using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace AE.Core.WPF.Controls
{
    public enum HorizontalItemsAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Stretch = 3,
        CenterLeft = 4,
        CenterRight = 5,
        CenterStretch = 6,
    }

    public class FlexItemsControl : ItemsControl
    {
        protected class ItemPosition
        {
            public FrameworkElement Element { get; }

            public double Left { get; set; }
            public double Top { get; set; }


            public ItemPosition(FrameworkElement element)
            {
                Element = element;

                Left = element.Margin.Left;
                Top = element.Margin.Top;
            }

            public ItemPosition(FrameworkElement element, int left, int top)
            {
                Element = element;

                Left = left;
                Top = top;
            }
        }

        public static readonly DependencyProperty HorizontalItemsAlignmentProperty =
            DependencyProperty.Register(nameof(HorizontalItemsAlignment), typeof(HorizontalItemsAlignment), typeof(FlexItemsControl), new PropertyMetadata((s, e) => (s as FlexItemsControl).Update()));

        public static readonly DependencyProperty IgnoreUpdateProperty =
            DependencyProperty.Register(nameof(IgnoreUpdate), typeof(bool), typeof(FlexItemsControl), new PropertyMetadata((s, e) => (s as FlexItemsControl).Update()));

        public HorizontalItemsAlignment HorizontalItemsAlignment
        {
            get => (HorizontalItemsAlignment)GetValue(HorizontalItemsAlignmentProperty);
            set => SetValue(HorizontalItemsAlignmentProperty, value);
        }

        public bool IgnoreUpdate
        {
            get => (bool)GetValue(IgnoreUpdateProperty);
            set => SetValue(IgnoreUpdateProperty, value);
        }

        public FlexItemsControl()
        {
            ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Grid)));
        }

        public virtual void Update()
        {
            if (IgnoreUpdate || Items == null || Items.Count == 0)
                return;

            var matrix = new List<int[]> { new[] { 0, (int)ActualWidth, 0 } };
            var data = new List<ItemPosition>();

            foreach (var item in Items)
            {
                var element = GetFrameworkItem(item);
                if (element != null)
                {
                    var size = new[] { (int)element.ActualWidth, (int)element.ActualHeight };
                    var point = GetAttachPoint(matrix, size[0]);

                    matrix = UpdateAttachArea(matrix, point, size);

                    UpdateAlignment(element);
                    data.Add(new ItemPosition(element, point[0], point[1]));
                }
            }

            if (ActualWidth > 0 && HorizontalItemsAlignment != HorizontalItemsAlignment.Left)
            {
                var positions = GetItemsMatrix(data);
                var maxWidth = positions.Max(r => r.Last().Left + r.Last().Element.ActualWidth);

                foreach (var row in positions)
                {
                    var rowMaxWidth = row.Last().Left + row.Last().Element.ActualWidth;

                    if (HorizontalItemsAlignment == HorizontalItemsAlignment.Stretch)
                    {
                        for (var i = 1; i < row.Count; i++)
                        {
                            var change = (ActualWidth - rowMaxWidth) / row.Count * (i + 1);
                            row[i].Left += change;
                        }
                    }
                    else if (HorizontalItemsAlignment == HorizontalItemsAlignment.CenterStretch)
                    {
                        for (var i = 0; i < row.Count; i++)
                        {
                            var change = ((ActualWidth - maxWidth) / (row.Count + 1) + (maxWidth - rowMaxWidth) / (row.Count + 1)) * (i + 1);
                            row[i].Left += change;
                        }
                    }
                    else
                    {
                        var change = HorizontalItemsAlignment switch
                        {
                            HorizontalItemsAlignment.Center => (ActualWidth - rowMaxWidth) / 2,
                            HorizontalItemsAlignment.CenterLeft => (ActualWidth - maxWidth) / 2,
                            HorizontalItemsAlignment.Right => ActualWidth - rowMaxWidth,
                            HorizontalItemsAlignment.CenterRight => (ActualWidth - maxWidth) / 2 + (maxWidth - rowMaxWidth),
                            _ => 0.0
                        };

                        if (change > 0)
                        {
                            foreach (var position in row.Skip(HorizontalItemsAlignment == HorizontalItemsAlignment.Stretch ? 1 : 0))
                                position.Left += change;
                        }
                    }
                }

                data = positions.SelectMany(p => p).ToList();
            }

            foreach (var item in data)
                SetPosition(item.Element, item.Left, item.Top);
        }

        protected static List<List<ItemPosition>> GetItemsMatrix(List<ItemPosition> items)
        {
            if (items == null || items.Count == 0)
                return [];

            var current = items[0];
            var result = new List<List<ItemPosition>>
            {
                new() { items[0] }
            };

            foreach (var position in items.Skip(1))
            {
                if (position.Top != current.Top)
                    result.Add([position]);
                else
                    result.Last().Add(position);

                current = position;
            }

            return result;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue == null)
            {
                base.OnItemsSourceChanged(oldValue, Array.Empty<object>());
                Update();
            }
            else
            {
                var newValueArray = newValue as object[] ?? newValue.Cast<object>().ToArray();
                base.OnItemsSourceChanged(oldValue, newValueArray);

                if (newValueArray != null && newValueArray.Length != 0)
                {
                    var element = GetFrameworkItem(newValueArray.Last());
                    HandleUpdate(element);
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Update();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                var element = GetFrameworkItem(e.NewItems.Cast<object>().Last());
                HandleUpdate(element);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
                Update();
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            HandleChildDesiredSizeChanged(child);
        }

        protected virtual void HandleChildDesiredSizeChanged(UIElement child)
        {
            if (child is FrameworkElement frameworkElement)
                HandleUpdate(frameworkElement);
        }

        protected virtual void HandleUpdate(FrameworkElement element)
        {
            if (element != null)
            {
                if (element.IsLoaded)
                    Update();
                else
                    element.Loaded += OnElementLoaded;
            }
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Loaded -= OnElementLoaded;
            Update();
        }

        protected virtual void SetPosition(FrameworkElement element, double newLeft, double newTop)
        {
            if (element != null)
                element.Margin = new Thickness(newLeft, newTop, 0, 0);
        }

        protected virtual void UpdateAlignment(FrameworkElement element)
        {
            if (element != null)
            {
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        protected virtual FrameworkElement GetFrameworkItem(object item)
        {
            if (item is FrameworkElement frameworkElement1)
                return frameworkElement1;

            if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement frameworkElement2)
                return frameworkElement2;

            return null;
        }

        protected static int[] GetAttachPoint(List<int[]> mtx, int width)
        {
            mtx.Sort(MatrixSortDepth);
            var max = mtx[^1][2];

            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtx[i][2] >= max)
                    break;

                if (mtx[i][1] - mtx[i][0] >= width)
                    return [mtx[i][0], mtx[i][2]];
            }

            return new[] { 0, max };
        }

        protected static List<int[]> UpdateAttachArea(List<int[]> mtx, int[] point, int[] size)
        {
            mtx.Sort(MatrixSortDepth);

            int[] cell = { point[0], point[0] + size[0], point[1] + size[1] };
            for (int i = 0, length = mtx.Count; i < length; i++)
                if (mtx.Count - 1 >= i)
                {
                    if (cell[0] <= mtx[i][0] && mtx[i][1] <= cell[1])
                        mtx.RemoveAt(i);
                    else
                        mtx[i] = MatrixTrimWidth(mtx[i], cell);
                }

            return MatrixJoin(mtx, cell);
        }

        protected static int MatrixSortDepth(int[] a, int[] b)
        {
            return (a[2] == b[2] && a[0] > b[0]) || a[2] > b[2] ? 1 : -1;
        }

        protected static int[] MatrixTrimWidth(int[] a, int[] b)
        {
            if (a[0] >= b[0] && a[0] < b[1] || a[1] >= b[0] && a[1] < b[1])
            {
                if (a[0] >= b[0] && a[0] < b[1])
                    a[0] = b[1];
                else
                    a[1] = b[0];
            }

            return a;
        }

        protected static List<int[]> MatrixJoin(List<int[]> mtx, int[] cell)
        {
            mtx.Add(cell);
            mtx.Sort(MatrixSortX);

            var mtxJoin = new List<int[]>();
            for (int i = 0, length = mtx.Count; i < length; i++)
            {
                if (mtxJoin.Count > 0 && mtxJoin[^1][1] == mtx[i][0] && mtxJoin[^1][2] == mtx[i][2])
                    mtxJoin[^1][1] = mtx[i][1];
                else
                    mtxJoin.Add(mtx[i]);
            }

            return mtxJoin;
        }

        protected static int MatrixSortX(int[] a, int[] b)
        {
            return a[0] > b[0] ? 1 : -1;
        }
    }
}

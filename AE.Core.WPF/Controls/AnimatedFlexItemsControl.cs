using System.Windows;
using System.Windows.Media.Animation;

namespace AE.Core.WPF.Controls
{
    public class AnimatedFlexItemsControl : FlexItemsControl
    {
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(AnimatedFlexItemsControl),
                new PropertyMetadata(new Duration(TimeSpan.FromSeconds(0.2)))
            );

        public Duration AnimationDuration
        {
            get => (Duration)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        private bool requestUpdate;
        protected AnimationManager AnimationManager { get; }

        public AnimatedFlexItemsControl()
        {
            AnimationManager = new AnimationManager(MarginProperty);
            AnimationManager.Completed += OnAnimationManagerCompleted;
        }

        private void OnAnimationManagerCompleted(object sender, EventArgs eventArgs)
        {
            if (requestUpdate)
            {
                requestUpdate = false;
                Update();
            }
        }

        public override void Update()
        {
            base.Update();
            AnimationManager.Start();
        }

        protected override void HandleChildDesiredSizeChanged(UIElement child)
        {
            if (AnimationManager.IsRunning)
                requestUpdate = true;
            else
                base.HandleChildDesiredSizeChanged(child);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (AnimationManager.IsRunning)
                requestUpdate = true;
            else
                base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void SetPosition(FrameworkElement element, double newLeft, double newTop)
        {
            if (element != null)
                AnimationManager.Enqueue(element, CreateAnimation(element, newLeft, newTop));
        }

        protected virtual AnimationTimeline CreateAnimation(FrameworkElement element, double newLeft, double newTop)
        {
            return new ThicknessAnimation
            {
                From = element.Margin,
                To = new Thickness(newLeft, newTop, 0, 0),
                Duration = AnimationDuration
            };
        }
    }
}

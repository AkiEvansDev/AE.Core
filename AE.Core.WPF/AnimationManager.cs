using System.Windows;
using System.Windows.Media.Animation;

namespace AE.Core.WPF
{
    public class AnimationItem(FrameworkElement element, AnimationTimeline animation)
    {
        public bool Cancel { get; set; }

        public FrameworkElement Element { get; set; } = element;
        public AnimationTimeline Animation { get; set; } = animation;
    }

    public class AnimationManager(DependencyProperty animationProperty)
    {
        public event EventHandler Completed;

        private readonly DependencyProperty AnimationProperty = animationProperty;
        private readonly List<AnimationItem> ActiveAnimations = [];
        private readonly Queue<AnimationItem> AnimationQueue = new();

        public bool IsRunning => ActiveAnimations.Count != 0;

        public void Enqueue(FrameworkElement element, AnimationTimeline animation)
        {
            if (element != null && animation != null)
            {
                AnimationQueue.Enqueue(new AnimationItem(element, animation));
            }
        }

        public void Start()
        {
            while (AnimationQueue.Count != 0)
            {
                var animationItem = AnimationQueue.Dequeue();
                if (animationItem != null && !animationItem.Cancel && animationItem.Element.IsLoaded)
                {
                    ClearActiveAnimations(animationItem);
                    HandleAnimationEvent(animationItem);

                    animationItem.Element.BeginAnimation(AnimationProperty, animationItem.Animation);
                }
            }
        }

        protected void ClearActiveAnimations(AnimationItem animationItem)
        {
            foreach (var activeAnimation in ActiveAnimations.ToArray())
            {
                if (activeAnimation.Element.Equals(animationItem.Element))
                    ActiveAnimations.Remove(activeAnimation);
            }
        }

        protected void HandleAnimationEvent(AnimationItem animationItem)
        {
            foreach (var a in ActiveAnimations.ToArray())
            {
                if (a.Element.Equals(animationItem.Element))
                    ActiveAnimations.Remove(a);
            }

            ActiveAnimations.Add(animationItem);
            animationItem.Animation.Completed += (s, e) =>
            {
                ActiveAnimations.Remove(animationItem);
                if (ActiveAnimations.Count == 0)
                    Completed?.Invoke(null, EventArgs.Empty);
            };
        }
    }
}

﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays
{
    public class NotificationManager : OsuFocusedOverlayContainer
    {
        private const float width = 320;

        public const float TRANSITION_LENGTH = 600;

        private ScrollContainer scrollContainer;
        private FlowContainer<NotificationSection> sections;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load()
        {
            Width = width;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                scrollContainer = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = Toolbar.Toolbar.HEIGHT },
                    Children = new[]
                    {
                        sections = new FillFlowContainer<NotificationSection>
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Children = new[]
                            {
                                new NotificationSection
                                {
                                    Title = @"Notifications",
                                    ClearText = @"Clear All",
                                    AcceptTypes = new[] { typeof(SimpleNotification) },
                                },
                                new NotificationSection
                                {
                                    Title = @"Running Tasks",
                                    ClearText = @"Cancel All",
                                    AcceptTypes = new[] { typeof(ProgressNotification) },
                                },
                            }
                        }
                    }
                }
            };
        }

        private int runningDepth;

        public void Post(Notification notification)
        {
            State = Visibility.Visible;

            ++runningDepth;
            notification.Depth = notification.DisplayOnTop ? runningDepth : -runningDepth;

            var hasCompletionTarget = notification as IHasCompletionTarget;
            if (hasCompletionTarget != null)
                hasCompletionTarget.CompletionTarget = Post;

            var ourType = notification.GetType();
            sections.Children.FirstOrDefault(s => s.AcceptTypes.Any(accept => accept.IsAssignableFrom(ourType)))?.Add(notification);
        }

        protected override void PopIn()
        {
            base.PopIn();

            scrollContainer.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.MoveToX(0, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(1, TRANSITION_LENGTH / 2);
        }

        private void markAllRead()
        {
            sections.Children.ForEach(s => s.MarkAllRead());
        }

        protected override void PopOut()
        {
            base.PopOut();

            markAllRead();

            this.MoveToX(width, TRANSITION_LENGTH, Easing.OutQuint);
            this.FadeTo(0, TRANSITION_LENGTH / 2);
        }
    }
}
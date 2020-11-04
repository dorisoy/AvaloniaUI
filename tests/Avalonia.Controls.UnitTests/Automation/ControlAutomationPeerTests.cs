using System.Linq;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Moq;
using Xunit;

namespace Avalonia.Controls.UnitTests.Automation
{
    public class ControlAutomationPeerTests
    {
        [Fact]
        public void Creates_Children_For_Controls_In_Visual_Tree()
        {
            var panel = new Panel
            {
                Children =
                {
                    new Border(),
                    new Border(),
                },
            };

            var root = new AutomationTestRoot(panel);
            var target = ControlAutomationPeer.GetOrCreatePeer(panel);

            Assert.Equal(2, target.GetChildCount());
            Assert.Equal(
                panel.GetVisualChildren(),
                target.GetChildren().Cast<ControlAutomationPeer>().Select(x => x.Owner));
        }

        [Fact]
        public void Creates_Children_when_Controls_Attached_To_Visual_Tree()
        {
            var contentControl = new ContentControl
            {
                Template = new FuncControlTemplate<ContentControl>((o, ns) =>
                    new ContentPresenter
                    {
                        Name = "PART_ContentPresenter",
                        [!ContentPresenter.ContentProperty] = o[!ContentControl.ContentProperty],
                    }),
                Content = new Border(),
            };

            var root = new AutomationTestRoot(contentControl);
            var target = ControlAutomationPeer.GetOrCreatePeer(contentControl);

            Assert.Equal(0, target.GetChildCount());
            Assert.Empty(target.GetChildren());

            contentControl.Measure(Size.Infinity);

            Assert.Equal(1, target.GetChildCount());
            Assert.Equal(1, target.GetChildren().Count);
        }

        [Fact]
        public void Notifies_PlatformImpl_Of_Child_Added()
        {
            var panel = new Panel
            {
                Children =
                {
                    new Border(),
                    new Border(),
                },
            };

            var root = new AutomationTestRoot(panel);
            var target = ControlAutomationPeer.GetOrCreatePeer(panel);

            var platformImplMock = Mock.Get(target.PlatformImpl);
            platformImplMock.Invocations.Clear();

            panel.Children.Add(new Decorator());

            platformImplMock.Verify(x => x.StructureChanged());
        }

        [Fact]
        public void Notifies_PlatformImpl_Of_Child_Removed()
        {
            var panel = new Panel
            {
                Children =
                {
                    new Border(),
                    new Border(),
                },
            };

            var root = new AutomationTestRoot(panel);
            var target = ControlAutomationPeer.GetOrCreatePeer(panel);

            var platformImplMock = Mock.Get(target.PlatformImpl);
            platformImplMock.Invocations.Clear();

            panel.Children.RemoveAt(1);

            platformImplMock.Verify(x => x.StructureChanged());
        }

        [Fact]
        public void Notifies_PlatformImpl_Of_Child_Being_Attached_To_Visual_Tree()
        {
            var contentControl = new ContentControl
            {
                Template = new FuncControlTemplate<ContentControl>((o, ns) =>
                    new ContentPresenter
                    {
                        Name = "PART_ContentPresenter",
                        [!ContentPresenter.ContentProperty] = o[!ContentControl.ContentProperty],
                    }),
                Content = new Border(),
            };

            var root = new AutomationTestRoot(contentControl);
            var target = ControlAutomationPeer.GetOrCreatePeer(contentControl);

            Assert.Empty(target.GetChildren());

            var platformImplMock = Mock.Get(target.PlatformImpl);
            platformImplMock.Invocations.Clear();

            contentControl.Measure(Size.Infinity);

            platformImplMock.Verify(x => x.StructureChanged());
        }

        [Fact]
        public void Notifies_PlatformImpl_Of_Child_Being_Detached_From_Visual_Tree()
        {
            var contentControl = new ContentControl
            {
                Template = new FuncControlTemplate<ContentControl>((o, ns) =>
                    new ContentPresenter
                    {
                        Name = "PART_ContentPresenter",
                        [!ContentPresenter.ContentProperty] = o[!ContentControl.ContentProperty],
                    }),
                Content = new Border(),
            };

            var root = new AutomationTestRoot(contentControl);
            var target = ControlAutomationPeer.GetOrCreatePeer(contentControl);

            contentControl.Measure(Size.Infinity);

            Assert.Equal(1, target.GetChildren().Count);

            var platformImplMock = Mock.Get(target.PlatformImpl);
            platformImplMock.Invocations.Clear();

            contentControl.Template = null;
            contentControl.Measure(Size.Infinity);

            platformImplMock.Verify(x => x.StructureChanged());
        }

        [Fact]
        public void Updates_Children_When_VisualChildren_Added()
        {
            var panel = new Panel
            {
                Children =
                {
                    new Border(),
                    new Border(),
                },
            };

            var root = new AutomationTestRoot(panel);
            var target = ControlAutomationPeer.GetOrCreatePeer(panel);
            var children = target.GetChildren();

            Assert.Equal(2, children.Count);

            panel.Children.Add(new Decorator());

            children = target.GetChildren();
            Assert.Equal(3, children.Count);
        }

        [Fact]
        public void Updates_Children_When_VisualChildren_Removed()
        {
            var panel = new Panel
            {
                Children =
                {
                    new Border(),
                    new Border(),
                },
            };

            var root = new AutomationTestRoot(panel);
            var target = ControlAutomationPeer.GetOrCreatePeer(panel);
            var children = target.GetChildren();
            var toRemove = children[1];

            Assert.Equal(2, children.Count);

            panel.Children.RemoveAt(1);

            children = target.GetChildren();
            Assert.Equal(1, children.Count);
            Assert.True(toRemove.IsDisposed);
        }

        private class AutomationTestRoot : TopLevel
        {
            public AutomationTestRoot() : base(CreateImpl())
            {
                Template = new FuncControlTemplate<AutomationTestRoot>((x, ns) => new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [!ContentPresenter.ContentProperty] = x[!ContentProperty],
                });
                ApplyTemplate();
            }

            public AutomationTestRoot(IControl child) : this() { Content = child; }

            private static ITopLevelImpl CreateImpl()
            {
                var mock = new Mock<ITopLevelImpl>();
                mock.Setup(x => x.RenderScaling).Returns(1);

                var ifs = mock.As<IPlatformAutomationInterface>();
                ifs.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                    .Returns(() => Mock.Of<IAutomationPeerImpl>());
                return mock.Object;
            }
        }
    }
}

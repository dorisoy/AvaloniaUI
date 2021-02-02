using System.Linq;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.UnitTests;
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
        }

        public class PlatformImpl
        {
            [Fact]
            public void PlatformImpl_Is_Created_For_Rooted_Control()
            {
                var target = new TestControl();
                var root = new AutomationTestRoot(target);
                var peer = ControlAutomationPeer.GetOrCreatePeer(target);

                Assert.IsType<TestAutomationPeer>(peer);
                Assert.NotNull(peer.PlatformImpl);
                Assert.NotNull(Mock.Get(peer.PlatformImpl));
            }

            [Fact]
            public void Detched_PlatformImpl_Is_Created_For_Nonrooted_Control()
            {
                var target = new TestControl();
                var peer = ControlAutomationPeer.GetOrCreatePeer(target);

                Assert.NotNull(peer.PlatformImpl);
                Assert.IsType<ControlAutomationPeer.DetachedPlatformImpl>(peer.PlatformImpl);
            }

            [Fact]
            public void PlatformImpl_Is_Disposed_When_Removed_From_Tree()
            {
                var target = new TestControl();
                var root = new AutomationTestRoot(target);
                var peer = ControlAutomationPeer.GetOrCreatePeer(target);
                var peerImplMock = Mock.Get(peer.PlatformImpl);

                root.Content = null;

                peerImplMock.Verify(x => x.Dispose());
                Assert.IsType<ControlAutomationPeer.DetachedPlatformImpl>(peer.PlatformImpl);
            }

            [Fact]
            public void PlatformImpl_Is_Recreated_When_Reattached_To_Tree()
            {
                var target = new TestControl();
                var root = new AutomationTestRoot(target);
                var peer = ControlAutomationPeer.GetOrCreatePeer(target);
                var peerImpl1 = peer.PlatformImpl;

                root.Content = null;
                root.Content = target;

                var peerImpl2 = peer.PlatformImpl;

                Assert.NotNull(peerImpl1);
                Assert.NotNull(peerImpl2);
                Assert.NotSame(peerImpl1, peerImpl2);
                Assert.IsNotType<ControlAutomationPeer.DetachedPlatformImpl>(peerImpl1);
                Assert.IsNotType<ControlAutomationPeer.DetachedPlatformImpl>(peerImpl2);
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
            public void PlatformImpl_Is_Disposed_When_Owning_Popup_Is_Closed()
            {
                using var app = UnitTestApplication.Start(TestServices.StyledWindow);
                var button = new Button();
                var popup = new Popup { Child = button };
                var root = new AutomationTestRoot(popup);

                popup.IsOpen = true;

                var target = ControlAutomationPeer.GetOrCreatePeer(button);
                var platformImplMock = Mock.Get(target.PlatformImpl);

                platformImplMock.Verify(x => x.Dispose(), Times.Never);

                popup.IsOpen = false;

                platformImplMock.Verify(x => x.Dispose(), Times.Once);
            }
        }

        private class TestControl : Control
        {
            protected override AutomationPeer OnCreateAutomationPeer() => new TestAutomationPeer(this);
        }

        private class TestAutomationPeer : ControlAutomationPeer
        {
            public TestAutomationPeer(Control owner) : base(owner, AutomationRole.Custom) { }
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

            private static IWindowBaseImpl CreateImpl()
            {
                var windowImpl = MockWindowingPlatform.CreateWindowMock();
                var windowAutomation = windowImpl.As<IPlatformAutomationInterface>();

                windowAutomation.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                    .Returns(() => Mock.Of<IAutomationPeerImpl>());

                windowImpl.Setup(x => x.CreatePopup()).Returns(() =>
                {
                    var popupImpl = MockWindowingPlatform.CreatePopupMock(windowImpl.Object);
                    var popupAutomation = popupImpl.As<IPlatformAutomationInterface>();
                    popupAutomation.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                        .Returns(() => Mock.Of<IAutomationPeerImpl>());
                    return popupImpl.Object;
                });

                return windowImpl.Object;
            }

            private static void AddAutomationFactory(Mock<IWindowImpl> impl)
            {
                var automationInterface = impl.As<IPlatformAutomationInterface>();
                automationInterface.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                    .Returns(() => Mock.Of<IAutomationPeerImpl>());
            }
        }
    }
}

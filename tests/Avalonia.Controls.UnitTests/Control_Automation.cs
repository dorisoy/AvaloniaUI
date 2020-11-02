using System;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Platform;
using Moq;
using Xunit;

namespace Avalonia.Controls.UnitTests
{
    public class Control_Automation
    {
        [Fact]
        public void Peer_Is_Created_For_Rooted_Control()
        {
            var target = new TestControl();
            var root = new AutomationTestRoot(target);
            var peer = ControlAutomationPeer.GetOrCreatePeer(target);

            Assert.IsType<TestAutomationPeer>(peer);
            Assert.NotNull(peer.PlatformImpl);
        }

        [Fact]
        public void Cannot_Get_Peer_For_Nonrooted_Control()
        {
            var target = new TestControl();
            Assert.Throws<InvalidOperationException>(() => ControlAutomationPeer.GetOrCreatePeer(target));
        }

        [Fact]
        public void Peer_Is_Disposed_When_Removed_From_Tree()
        {
            var target = new TestControl();
            var root = new AutomationTestRoot(target);
            var peer = ControlAutomationPeer.GetOrCreatePeer(target);

            root.Content = null;

            Assert.True(peer.IsDisposed);
            Assert.Null(peer.PlatformImpl);
            Assert.Throws<InvalidOperationException>(() => ControlAutomationPeer.GetOrCreatePeer(target));
        }

        [Fact]
        public void Peer_Is_Recreated_When_Reattached_To_Tree()
        {
            var target = new TestControl();
            var root = new AutomationTestRoot(target);
            var peer1 = ControlAutomationPeer.GetOrCreatePeer(target);

            root.Content = null;
            root.Content = target;

            var peer2 = ControlAutomationPeer.GetOrCreatePeer(target);

            Assert.NotNull(peer1);
            Assert.NotNull(peer2);
            Assert.NotSame(peer1, peer2);
        }

        private class TestControl : Control
        {
            protected override AutomationPeer OnCreateAutomationPeer() => new TestAutomationPeer(this);
        }

        private class TestAutomationPeer : ControlAutomationPeer
        {
            public TestAutomationPeer(Control owner) : base(owner) { }
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
                var ifs = mock.As<IPlatformAutomationInterface>();
                ifs.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                    .Returns(() => Mock.Of<IAutomationPeerImpl>());
                return mock.Object;
            }
        }
    }
}

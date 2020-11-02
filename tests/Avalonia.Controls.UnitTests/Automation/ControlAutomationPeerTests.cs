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
        public void Creates_Children()
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

            Assert.Equal(
                panel.GetVisualChildren(),
                target.GetChildren().Cast<ControlAutomationPeer>().Select(x => x.Owner));
        }

        [Fact]
        public void Notifies_PlatformImpl_Of_Change_To_Children()
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

            panel.Children.Add(new Decorator());

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
                var ifs = mock.As<IPlatformAutomationInterface>();
                ifs.Setup(x => x.CreateAutomationPeerImpl(It.IsAny<AutomationPeer>()))
                    .Returns(() => Mock.Of<IAutomationPeerImpl>());
                return mock.Object;
            }
        }
    }
}

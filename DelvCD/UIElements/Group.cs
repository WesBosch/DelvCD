using DelvCD.Config;
using DelvCD.Helpers;
using DelvCD.Helpers.DataSources;
using System.Collections.Generic;
using System.Numerics;

namespace DelvCD.UIElements
{
    public class Group : UIElement, IGroup
    {
        public override ElementType Type => ElementType.Group;

        public ElementListConfig ElementList { get; set; }

        public GroupConfig GroupConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public Group() : this(string.Empty) { }

        public Group(string name) : base(name)
        {
            ElementList = new ElementListConfig();
            GroupConfig = new GroupConfig();
            VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return ElementList;
            yield return GroupConfig;
            yield return VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case ElementListConfig newPage:
                    ElementList = newPage;
                    break;
                case GroupConfig newPage:
                    GroupConfig = newPage;
                    break;
                case VisibilityConfig newPage:
                    VisibilityConfig = newPage;
                    break;
            }
        }

        public override void StopPreview()
        {
            base.StopPreview();

            foreach (UIElement element in ElementList.UIElements)
            {
                element.StopPreview();
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null, bool parentVisible = true)
        {
            bool visible = VisibilityConfig.IsVisible(parentVisible);
            foreach (UIElement element in ElementList.UIElements)
            {
                if (!Preview && LastFrameWasPreview)
                {
                    element.Preview = false;
                }
                else
                {
                    element.Preview |= Preview;
                }

                if (visible || Singletons.Get<PluginManager>().IsConfigOpen())
                {
                    element.Draw(pos + GroupConfig.Position, null, visible);

                    if (this.GroupConfig._keepsorted)
                    {
                        RepositionElements(GroupConfig.DynamicBuffer, GroupConfig._dynamicConditions, GroupConfig._elementCount);
                    }
                }

            }

            LastFrameWasPreview = Preview;
        }

        public void MakeDynamic(bool conditions)
        {
            foreach (UIElement item in ElementList.UIElements)
            {
                if (item is Icon icon)
                {
                    icon.MakeDynamic(conditions);
                }

                if (item is Bar bar)
                {
                    bar.MakeDynamic(conditions);
                }

                if (item is Group group)
                {
                    group.MakeDynamicSub();
                }
            }
        }
        public void MakeDynamicSub()
        {
            GroupConfig.IsDynamic = true;
        }

        public void MarkParentDynamic(bool conditions, bool toggle)
        {
            foreach (UIElement item in ElementList.UIElements)
            {
                if (item is Icon icon)
                {
                    icon.MakeParentDynamic(conditions, toggle);
                }

                if (item is Bar bar)
                {
                    bar.MakeParentDynamic(conditions, toggle);
                }

                if (item is Group group)
                {
                    group.MakeParentDynamicSub(toggle);
                }
            }
        }

        public void MakeParentDynamicSub(bool toggle)
        {
            GroupConfig.IsParentDynamic = toggle;
        }

        public void Reposition(Vector2 pos, int elementCount)
        {
            if (GroupConfig.IsDynamic == true)
            {
                pos = new Vector2((pos.X * elementCount), (pos.Y * elementCount));
                GroupConfig.Position = pos;
            }
        }

        public void RepositionElements(Vector2 buffer, bool conditions, int elementCount)
        {
            foreach (UIElement item in ElementList.UIElements)
            {
                if (item is Icon icon)
                {
                    bool istriggered = icon.TriggerConfig.IsTriggered(this.Preview, out int triggeredIndex);
                    if (istriggered)
                    {
                        elementCount++;
                        icon.Reposition(buffer, conditions, elementCount-1);
                    }
                }

                if (item is Bar bar)
                {
                    bool istriggered = bar.TriggerConfig.IsTriggered(this.Preview, out int triggeredIndex);
                    if (istriggered)
                    {
                        elementCount++;
                        bar.Reposition(buffer, conditions, elementCount-1);
                    }
                }

                if (item is Group group)
                {
                    bool istriggered = false;
                    if (group.Preview || AreSubitemsVisible(group)) { istriggered = true; }

                    if (istriggered)
                    {
                        elementCount++;
                        group.Reposition(buffer, elementCount-1);
                    }
                }
            }
        }

        public bool AreSubitemsVisible(Group group)
        {
            foreach (UIElement subitem in group.ElementList.UIElements)
            {
                if (subitem is Icon subicon)
                {
                    bool issubtriggered = subicon.TriggerConfig.IsTriggered(this.Preview, out int triggeredIndex);
                    if (issubtriggered)
                    {
                        return true;
                    }
                }

                if (subitem is Bar subbar)
                {
                    bool issubtriggered = subbar.TriggerConfig.IsTriggered(this.Preview, out int triggeredIndex);
                    if (issubtriggered)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ResizeIcons(Vector2 size, bool recurse, bool conditions)
        {
            foreach (UIElement item in ElementList.UIElements)
            {
                if (item is Icon icon)
                {
                    icon.Resize(size, conditions);
                }
                else if (recurse && item is Group group)
                {
                    group.ResizeIcons(size, recurse, conditions);
                }
            }
        }

        public void ScaleResolution(Vector2 scaleFactor, bool positionOnly)
        {
            GroupConfig.Position *= scaleFactor;
            foreach (UIElement item in ElementList.UIElements)
            {
                if (item is Icon icon)
                {
                    icon.ScaleResolution(scaleFactor, positionOnly);
                }
                else if (item is Group group)
                {
                    group.ScaleResolution(scaleFactor, positionOnly);
                }
            }
        }
    }
}
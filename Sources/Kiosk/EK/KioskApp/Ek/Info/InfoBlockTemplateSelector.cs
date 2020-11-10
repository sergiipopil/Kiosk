using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Info
{
    public class InfoBlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Paragraph { get; set; }

        public DataTemplate ExtraSpace { get; set; }

        public DataTemplate ListItem { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var block = item as InfoBlock;
            switch (block?.Type)
            {
                case InfoBlockTypeEnum.Paragraph:
                    return Paragraph;

                case InfoBlockTypeEnum.ExtraSpace:
                    return ExtraSpace;

                case InfoBlockTypeEnum.ListItem:
                    return ListItem;

                default:
                    return null;
            }
        }
    }
}
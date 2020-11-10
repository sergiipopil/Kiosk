using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Helpers.Ui.Binding
{
    // todo: all methods should be tasks
    public class UiBindableCollection<TItem> : ObservableCollection<TItem>
    {
        public UiBindableCollection()
        {
        }

        public UiBindableCollection(IEnumerable<TItem> baseCollection)
            : base(baseCollection)
        {
        }

        protected override void InsertItem(int index, TItem item)
        {
            ThreadHelper.RunInUiThreadAsync(() => base.InsertItem(index, item));
        }

        protected override void SetItem(int index, TItem item)
        {
            ThreadHelper.RunInUiThreadAsync(() => base.SetItem(index, item));
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            ThreadHelper.RunInUiThreadAsync(() => base.MoveItem(oldIndex, newIndex));
        }

        protected override void RemoveItem(int index)
        {
            ThreadHelper.RunInUiThreadAsync(() => base.RemoveItem(index));
        }

        protected override void ClearItems()
        {
            ThreadHelper.RunInUiThreadAsync(() => base.ClearItems());
        }

        public Task AddRangeAsync(IEnumerable<TItem> items)
        {
            Assure.ArgumentNotNull(items, nameof(items));

            return ThreadHelper.RunInUiThreadAsync(() =>
                {
                    foreach (var item in items)
                    {
                        Add(item);
                    }
                });
        }
    }
}
using System;
using System.ComponentModel;
using KioskApp.Ek.Catalog.Categories;

namespace KioskApp.Search
{
    public interface ICategorySearchProvider : ISearchStateProvider, INotifyPropertyChanged
    {
        string SearchTitle { get; }

        Category[] Breadcrumbs { get; }

        Category[] Categories { get; }

        void ChangeCategory(Category category);

        void SelectCategory(Category category);

        void GoBack();

        event EventHandler<SelectedCategoryValue> CategorySelected;
    }
}
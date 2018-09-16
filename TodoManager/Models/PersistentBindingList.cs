﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TodoManager.Helpers;

namespace TodoManager.Models
{
    public abstract class PersistentBindingList<T> : BindingList<T>
    {
        private readonly bool _loading;

        public PersistentBindingList()
        {
        }

        public PersistentBindingList(IList<T> items)
        {
            ParameterValidator.CheckNull(items, "items");
            try
            {
                _loading = true;
                foreach (var item in items)
                {
                    // must use Add (not Items.Add()) for bindings to work correctly
                    Add(item);
                }
            }
            finally
            {
                _loading = false;
            }
        }

        protected override async void InsertItem(int index, T item)
        {
            if (index != Count)
            {
                // we only append at this stage
                throw new NotSupportedException("Insert not supported.");
            }

            // do not go to the server if we are initially loading list
            T newItem = _loading ? item : await AddToStoreAsync(item);

            base.InsertItem(index, newItem);
        }

        protected abstract Task<T> AddToStoreAsync(T item);

        protected override async void RemoveItem(int index)
        {
            if (index < Count)
            {
                await RemoveFromStoreAsync(Items[index]);
            }
            
            base.RemoveItem(index);
        }

        protected abstract Task RemoveFromStoreAsync(T item);

        protected override async void ClearItems()
        {
            await RemoveAllFromStoreAsync();
            base.ClearItems();
        }

        protected abstract Task RemoveAllFromStoreAsync();

        protected override async void OnListChanged(ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                await UpdateStoreAsync(Items[e.NewIndex]);
            }
            base.OnListChanged(e);
        }

        protected abstract Task UpdateStoreAsync(T item);
    }
}

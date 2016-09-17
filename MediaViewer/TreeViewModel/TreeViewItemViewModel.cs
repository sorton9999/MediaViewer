using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MediaViewer.TreeViewModel
{
    /// <summary>
    /// The parent class of the Tree View Model that provides the tree handling
    /// methods and data structures as well as a lazy loading mechanism.  This
    /// class is a subclass of ViewModelBase which provides the Property
    /// notification functionality.
    /// </summary>
    public class TreeViewItemViewModel : ViewModelBase
    {
        #region Selected Item Delegate/Event
        public delegate void SelectedItemDelegateHandler(TreeViewItemViewModel item);
        public static event SelectedItemDelegateHandler OnItemSelected = delegate { };
        #endregion

        #region Data
        static readonly TreeViewItemViewModel DummyChild = new TreeViewItemViewModel();

        readonly ObservableCollection<TreeViewItemViewModel> _children;
        readonly TreeViewItemViewModel _parent;

        private TreeViewItemViewModel _selectedItem;

        bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            _parent = parent;

            _children = new ObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        #region Presentation Members

        #region Children

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get { return _children; }
        }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        #endregion // HasLoadedChildren

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.NotifyPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.NotifyPropertyChanged("IsSelected");
                    SelectedItem = this;
                }
            }
        }

        #endregion // IsSelected

        #region LoadChildren

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        #endregion // LoadChildren

        #region Parent

        public TreeViewItemViewModel Parent
        {
            get { return _parent; }
        }

        #endregion // Parent

        #endregion // Presentation Members

        #region Selected Item

        public TreeViewItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnSelectedItemChanged();
                }
            }
        }

        private void OnSelectedItemChanged()
        {
            OnItemSelected(_selectedItem);
        }

        #endregion
    }
}

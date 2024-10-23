using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MuPDFCore
{
    /// <summary>
    /// Contains information about optional content groups in a PDF document (also known as layers).
    /// </summary>
    public class MuPDFOptionalContentGroupData
    {
        /// <summary>
        /// The default optional content group configuration.
        /// </summary>
        public MuPDFOptionalContentGroupConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// Alternative optional content group configurations.
        /// </summary>
        public MuPDFOptionalContentGroupConfiguration[] AlternativeConfigurations { get; }

        /// <summary>
        /// All optional content groups (layers) defined in this document.
        /// </summary>
        public MuPDFOptionalContentGroup[] OptionalContentGroups { get; }
        private MuPDFDocument OwnerDocument { get; }

        internal static MuPDFOptionalContentGroupData Load(MuPDFDocument ownerDocument)
        {
            if (ownerDocument.NativePDFDocument != IntPtr.Zero)
            {
                MuPDFOptionalContentGroupData tbr = new MuPDFOptionalContentGroupData(ownerDocument);

                if (tbr.DefaultConfiguration != null || tbr.AlternativeConfigurations.Length > 0 || tbr.OptionalContentGroups.Length > 0)
                {
                    return tbr;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private unsafe MuPDFOptionalContentGroupData(MuPDFDocument ownerDocument)
        {
            this.OwnerDocument = ownerDocument;
            this.DefaultConfiguration = MuPDFOptionalContentGroupConfiguration.GetDefaultConfiguration(OwnerDocument);

            int alternativeOcgConfigs = NativeMethods.CountAlternativeOCGConfigs(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument);

            this.AlternativeConfigurations = new MuPDFOptionalContentGroupConfiguration[alternativeOcgConfigs];

            for (int i = 0; i < alternativeOcgConfigs; i++)
            {
                this.AlternativeConfigurations[i] = MuPDFOptionalContentGroupConfiguration.GetConfiguration(OwnerDocument, i);
            }

            this.DefaultConfiguration?.Activate();

            int ocgCount = NativeMethods.CountOptionalContentGroups(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument);

            this.OptionalContentGroups = new MuPDFOptionalContentGroup[ocgCount];

            if (ocgCount > 0)
            {
                int[] ocgNameLengths = new int[ocgCount];

                fixed (int* ocgNameLengthPtr = ocgNameLengths)
                {
                    NativeMethods.GetOptionalContentGroupNameLengths(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, ocgCount, (IntPtr)ocgNameLengthPtr);
                }

                byte[][] ocgNames = new byte[ocgCount][];
                GCHandle[] ocgNameHandles = new GCHandle[ocgCount];
                IntPtr[] ocgNamePointers = new IntPtr[ocgCount];

                for (int i = 0; i < ocgCount; i++)
                {
                    ocgNames[i] = new byte[ocgNameLengths[i]];
                    ocgNameHandles[i] = GCHandle.Alloc(ocgNames[i], GCHandleType.Pinned);
                    ocgNamePointers[i] = ocgNameHandles[i].AddrOfPinnedObject();
                }

                fixed (IntPtr* ocgNamePointersPointer = ocgNamePointers)
                {
                    NativeMethods.GetOptionalContentGroups(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, ocgCount, (IntPtr)ocgNamePointersPointer);
                }

                for (int i = 0; i < ocgCount; i++)
                {
                    ocgNameHandles[i].Free();
                }

                for (int i = 0; i < ocgCount; i++)
                {
                    this.OptionalContentGroups[i] = new MuPDFOptionalContentGroup(this.OwnerDocument, Encoding.ASCII.GetString(ocgNames[i]), i);
                }
            }
        }
    }

    /// <summary>
    /// Represents an optional content group configuration.
    /// </summary>
    public class MuPDFOptionalContentGroupConfiguration
    {
        /// <summary>
        /// The name of this optional content group configuration.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The creator of this optional content group configuration.
        /// </summary>
        public string Creator { get; }

        /// <summary>
        /// Gets whether this is the default optional content group configuration for the document.
        /// </summary>
        public bool IsDefault { get => Index < 0; }

        /// <summary>
        /// Optional content group "UI" elements associated with this configuration.
        /// </summary>
        public MuPDFOptionalContentGroupUIItem[] UI { get; }

        private MuPDFDocument OwnerDocument { get; }
        private int Index { get; }

        private unsafe MuPDFOptionalContentGroupConfiguration(string name, string creator, int index, MuPDFDocument ownerDocument)
        {
            this.Name = name;
            this.Creator = creator;
            this.Index = index;
            this.OwnerDocument = ownerDocument;

            int uiCount = 0;

            if (this.Name != null || this.Creator != null)
            {
                this.Activate();
                uiCount = NativeMethods.CountOptionalContentGroupConfigUI(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument);
            }

            if (uiCount > 0)
            {
                int[] uiLabelLengths = new int[uiCount];

                fixed (int* uiLabelLengthsPtr = uiLabelLengths)
                {
                    NativeMethods.ReadOptionalContentGroupUILabelLengths(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, uiCount, (IntPtr)uiLabelLengthsPtr);
                }

                byte[][] labelBytes = new byte[uiCount][];
                GCHandle[] labelHandles = new GCHandle[uiCount];
                IntPtr[] labelPointers = new IntPtr[uiCount];
                for (int i = 0; i < uiCount; i++)
                {
                    labelBytes[i] = new byte[uiLabelLengths[i]];
                    labelHandles[i] = GCHandle.Alloc(labelBytes[i], GCHandleType.Pinned);
                    labelPointers[i] = labelHandles[i].AddrOfPinnedObject();
                }

                int[] depths = new int[uiCount];
                int[] types = new int[uiCount];
                int[] locked = new int[uiCount];

                fixed (IntPtr* labelPointersPtr = labelPointers)
                fixed (int* depthsPtr = depths)
                fixed (int* typesPtr = types)
                fixed (int* lockedPtr = locked)
                {
                    NativeMethods.ReadOptionalContentGroupUIs(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, uiCount, (IntPtr)labelPointersPtr, (IntPtr)depthsPtr, (IntPtr)typesPtr, (IntPtr)lockedPtr);
                }

                string[] labels = new string[uiCount];
                for (int i = 0; i < uiCount; i++)
                {
                    labels[i] = Encoding.ASCII.GetString(labelBytes[i]);
                }

                TemporaryUIItem superRootItem = new TemporaryUIItem(null, -1, -1, -1, -1, null);
                TemporaryUIItem rootItem = new TemporaryUIItem(null, -1, -1, 0, -1, superRootItem);
                TemporaryUIItem currItem = rootItem;

                for (int i = 0; i < uiCount; i++)
                {
                    if (types[i] == 0) // Label
                    {
                        while (currItem.Depth >= depths[i])
                        {
                            currItem = currItem.Parent;
                        }

                        TemporaryUIItem temp = new TemporaryUIItem(labels[i], types[i], locked[i], depths[i], i, currItem);
                        currItem.Children.Add(temp);
                        currItem = temp;
                    }
                    else
                    {
                        while (!((currItem.Depth == depths[i] && currItem.Type == 0) || (currItem.Depth == depths[i] - 1 && currItem.Type != 0)))
                        {
                            currItem = currItem.Parent;
                        }

                        TemporaryUIItem temp = new TemporaryUIItem(labels[i], types[i], locked[i], depths[i], i, currItem);
                        currItem.Children.Add(temp);
                        currItem = temp;
                    }
                }

                for (int i= 0; i < superRootItem.Children.Count; i++)
                {
                    if (superRootItem.Children[i] != rootItem)
                    {
                        superRootItem.Children[i].Parent = rootItem;
                        rootItem.Children.Add(superRootItem.Children[i]);
                    }
                }

                rootItem.Parent = null;

                this.UI = new MuPDFOptionalContentGroupUIItem[rootItem.Children.Count];

                for (int i = 0; i < rootItem.Children.Count; i++)
                {
                    this.UI[i] = rootItem.Children[i].Build(ownerDocument);
                }
            }
            else
            {
                this.UI = new MuPDFOptionalContentGroupUIItem[0];
            }
        }

        private class TemporaryUIItem
        {
            public string Label { get; set; }
            public int Type { get; set; }
            public int IsLocked { get; set; }
            public List<TemporaryUIItem> Children { get; set; }
            public int Depth { get; set; }
            public TemporaryUIItem Parent { get; set; }
            public int Index { get; set; }

            public TemporaryUIItem(string label, int type, int isLocked, int depth, int index, TemporaryUIItem parent)
            {
                Label = label;
                Type = type;
                IsLocked = isLocked;
                Children = new List<TemporaryUIItem>();
                this.Depth = depth;
                this.Index = index;
                Parent = parent;
            }

            public MuPDFOptionalContentGroupUIItem Build(MuPDFDocument ownerDocument)
            {
                MuPDFOptionalContentGroupUIItem[] children = new MuPDFOptionalContentGroupUIItem[this.Children.Count];

                for (int i = 0; i < this.Children.Count; i++)
                {
                    children[i] = this.Children[i].Build(ownerDocument);
                }

                switch (this.Type)
                {
                    case 0:
                        return new MuPDFOptionalContentGroupLabel(this.Label, children);
                    case 1:
                        return new MuPDFOptionalContentGroupCheckbox(this.Label, children, this.IsLocked != 0, this.Index, ownerDocument);
                    case 2:
                        return new MuPDFOptionalContentGroupRadioButton(this.Label, children, this.IsLocked != 0, this.Index, ownerDocument);
                    default:
                        throw new ArgumentException("Unknown UI item type: " + this.Type.ToString());
                }
            }

        }

        internal static unsafe MuPDFOptionalContentGroupConfiguration GetDefaultConfiguration(MuPDFDocument ownerDocument)
        {
            int nameLength = 0;
            int creatorLength = 0;

            NativeMethods.ReadDefaultOCGConfigNameLength(ownerDocument.OwnerContext.NativeContext, ownerDocument.NativePDFDocument, ref nameLength, ref creatorLength);

            if (nameLength > 0 || creatorLength > 0)
            {
                byte[] configName = new byte[nameLength];
                byte[] configCreator = new byte[creatorLength];

                fixed (byte* configNamePtr = configName)
                fixed (byte* configCreatorPtr = configCreator)
                {
                    NativeMethods.ReadDefaultOCGConfig(ownerDocument.OwnerContext.NativeContext, ownerDocument.NativePDFDocument, nameLength, creatorLength, (IntPtr)configNamePtr, (IntPtr)configCreatorPtr);
                }

                return new MuPDFOptionalContentGroupConfiguration(Encoding.ASCII.GetString(configName), Encoding.ASCII.GetString(configCreator), -1, ownerDocument);
            }
            else
            {
                return null;
            }
        }

        internal static unsafe MuPDFOptionalContentGroupConfiguration GetConfiguration(MuPDFDocument ownerDocument, int index)
        {
            int nameLength = 0;
            int creatorLength = 0;

            NativeMethods.ReadOCGConfigNameLength(ownerDocument.OwnerContext.NativeContext, ownerDocument.NativePDFDocument, index, ref nameLength, ref creatorLength);

            if (nameLength > 0 || creatorLength > 0)
            {
                byte[] configName = new byte[nameLength];
                byte[] configCreator = new byte[creatorLength];

                fixed (byte* configNamePtr = configName)
                fixed (byte* configCreatorPtr = configCreator)
                {
                    NativeMethods.ReadOCGConfig(ownerDocument.OwnerContext.NativeContext, ownerDocument.NativePDFDocument, index, nameLength, creatorLength, (IntPtr)configNamePtr, (IntPtr)configCreatorPtr);
                }

                return new MuPDFOptionalContentGroupConfiguration(Encoding.ASCII.GetString(configName), Encoding.ASCII.GetString(configCreator), index, ownerDocument);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Activates this optional content group configuration.
        /// </summary>
        public void Activate()
        {
            if (this.IsDefault)
            {
                NativeMethods.EnableDefaultOCGConfig(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument);
            }
            else
            {
                NativeMethods.EnableOCGConfig(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, this.Index);
            }
            this.OwnerDocument.ClearCache();
        }
    }

    /// <summary>
    /// Represents an optional content group (also known as layer).
    /// </summary>
    public class MuPDFOptionalContentGroup
    {
        private int Index { get; }
        private MuPDFDocument OwnerDocument { get; }

        /// <summary>
        /// The name of the optional content group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets whether the optional content group is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return NativeMethods.GetOptionalContentGroupState(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, this.Index) != 0;
            }
            set
            {
                NativeMethods.SetOptionalContentGroupState(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, this.Index, value ? 1 : 0);
                OwnerDocument.ClearCache();
            }
        }

        internal MuPDFOptionalContentGroup(MuPDFDocument ownerDocument, string name, int index)
        {
            this.Name = name;
            this.Index = index;
            this.OwnerDocument = ownerDocument;
        }
    }

    /// <summary>
    /// An optional content group UI element.
    /// </summary>
    public abstract class MuPDFOptionalContentGroupUIItem
    {
        /// <summary>
        /// The label for this optional content group UI element.
        /// </summary>
        public virtual string Label { get; }

        /// <summary>
        /// Optional content group UI elements nested within this UI element (this may be empty, but it will not be <see langword="null"/>.
        /// </summary>
        public virtual MuPDFOptionalContentGroupUIItem[] Children { get; }

        internal MuPDFOptionalContentGroupUIItem(string label, MuPDFOptionalContentGroupUIItem[] children)
        {
            Label = label;
            Children = children;
        }
    }

    /// <summary>
    /// An optional content group UI element that can be enabled or disabled.
    /// </summary>
    public abstract class MuPDFOptionalContentGroupSelectableUIItem : MuPDFOptionalContentGroupUIItem
    {
        /// <summary>
        /// Gets or sets whether the optional content group UI element is enabled or not.
        /// </summary>
        public virtual bool IsEnabled
        {
            get => NativeMethods.ReadOptionalContentGroupUIState(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativePDFDocument, this.Index) != 0;
            set
            {
                NativeMethods.SetOptionalContentGroupUIState(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativeDocument, this.Index, value ? 1 : 0);
                OwnerDocument.ClearCache();
            }
        }

        /// <summary>
        /// Indicates whether the state of the optional content group UI element is locked or can be changed.
        /// </summary>
        public virtual bool IsLocked { get; }

        private int Index { get; }

        private MuPDFDocument OwnerDocument { get; }

        /// <summary>
        /// Toggle the state of the optional content group UI element.
        /// </summary>
        public virtual void Toggle()
        {
            NativeMethods.SetOptionalContentGroupUIState(this.OwnerDocument.OwnerContext.NativeContext, this.OwnerDocument.NativeDocument, this.Index, 2);
            OwnerDocument.ClearCache();
        }

        internal MuPDFOptionalContentGroupSelectableUIItem(string label, MuPDFOptionalContentGroupUIItem[] children, bool isLocked, int index, MuPDFDocument ownerDocument) : base(label, children)
        {
            this.IsLocked = isLocked;
            this.Index = index;
            this.OwnerDocument = ownerDocument;
        }
    }

    /// <summary>
    /// An optional content group UI element that should be represented as a check box.
    /// </summary>
    public class MuPDFOptionalContentGroupCheckbox : MuPDFOptionalContentGroupSelectableUIItem
    {
        internal MuPDFOptionalContentGroupCheckbox(string label, MuPDFOptionalContentGroupUIItem[] children, bool isLocked, int index, MuPDFDocument ownerDocument) : base(label, children, isLocked, index, ownerDocument) { }
    }

    /// <summary>
    /// An optional content group UI element that should be represented as a radio button.
    /// </summary>
    public class MuPDFOptionalContentGroupRadioButton : MuPDFOptionalContentGroupSelectableUIItem
    {
        internal MuPDFOptionalContentGroupRadioButton(string label, MuPDFOptionalContentGroupUIItem[] children, bool isLocked, int index, MuPDFDocument ownerDocument) : base(label, children, isLocked, index, ownerDocument) { }
    }

    /// <summary>
    /// An optional content group UI element that should be represented as a label.
    /// </summary>
    public class MuPDFOptionalContentGroupLabel : MuPDFOptionalContentGroupUIItem
    {
        internal MuPDFOptionalContentGroupLabel(string label, MuPDFOptionalContentGroupUIItem[] children) : base(label, children) { }
    }
}

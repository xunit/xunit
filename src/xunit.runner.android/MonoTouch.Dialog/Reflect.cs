using System;

namespace MonoDroid.Dialog
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class EntryAttribute : Attribute
    {
        public string Placeholder;

        public EntryAttribute() : this(null)
        {
        }

        public EntryAttribute(string placeholder)
        {
            Placeholder = placeholder;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class DateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class TimeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class CheckboxAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class MultilineAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class HtmlAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class SkipAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class StringAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class PasswordAttribute : EntryAttribute
    {
        public PasswordAttribute(string placeholder) : base(placeholder)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class AlignmentAttribute : Attribute
    {
        public AlignmentAttribute(object alignment)
        {
            Alignment = alignment;
        }
        public object Alignment;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class RadioSelectionAttribute : Attribute
    {
        public string Target;

        public RadioSelectionAttribute(string target)
        {
            Target = target;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class OnTapAttribute : Attribute
    {
        public string Method;

        public OnTapAttribute(string method)
        {
            Method = method;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class CaptionAttribute : Attribute
    {
        public string Caption;

        public CaptionAttribute(string caption)
        {
            Caption = caption;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal class SectionAttribute : Attribute
    {
        public string Caption, Footer;

        public SectionAttribute()
        {
        }

        public SectionAttribute(string caption)
        {
            Caption = caption;
        }

        public SectionAttribute(string caption, string footer)
        {
            Caption = caption;
            Footer = footer;
        }
    }

    internal class RangeAttribute : Attribute
    {
        public int High;
        public int Low;
        public bool ShowCaption;

        public RangeAttribute(int low, int high)
        {
            Low = low;
            High = high;
        }
    }
}
// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Globalization;
using System.Windows;
using Stride.Core.Reflection;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Data;

namespace Stride.Core.Assets.Editor.View.ValueConverters
{
    public class EnumToTooltip : MarkupExtension, IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Value can be null when the control is removed from the visual tree and the related property is unbound.
            if (value == null || value == DependencyProperty.UnsetValue)
                return null;

            var stringValue = value.ToString();
            var member = value.GetType().GetMember(stringValue)[0];
            var attribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(member);
            return attribute != null
                ? (string.IsNullOrEmpty(attribute.Name)
                    ? attribute.Name
                    : stringValue)
                : stringValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new EnumToTooltip();
        }
    }
}

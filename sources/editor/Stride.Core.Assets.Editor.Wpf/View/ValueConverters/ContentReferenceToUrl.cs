// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Globalization;

using Stride.Core.Assets.Editor.Services;
using Stride.Core.Assets.Editor.ViewModel;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using Stride.Core.Presentation.Quantum.ViewModels;
using System.Windows.Markup;
using System.Windows.Data;

namespace Stride.Core.Assets.Editor.View.ValueConverters
{
    public class ContentReferenceToUrl : MarkupExtension, IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ContentReferenceHelper.EmptyReference;

            if (value == NodeViewModel.DifferentValues)
                return "(Different values)";

            var contentReference = value as IReference ?? AttachedReferenceManager.GetOrCreateAttachedReference(value);
            var asset = contentReference != null && contentReference.Id != AssetId.Empty ? SessionViewModel.Instance.GetAssetById(contentReference.Id) : null;
            return asset != null ? asset.Url : ContentReferenceHelper.BrokenReference;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = (string)value;
            var asset = SessionViewModel.Instance.AllAssets.FirstOrDefault(x => x.Url == url);
            if (asset == null)
                return null;

            var contentType = AssetRegistry.GetContentType(asset.AssetType);
            return AttachedReferenceManager.CreateProxyObject(contentType, asset.Id, asset.Url);
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new ContentReferenceToUrl();
        }
    }
}

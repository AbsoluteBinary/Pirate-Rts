//-----------------------------------------------------------------------
// <copyright file="DisallowAddressableSubAssetFieldAttributeValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

#if !SIRENIX_INTERNAL
#pragma warning disable
#endif

using Plugins.Sirenix.Odin_Inspector.Modules.Unity.Addressables.Validators;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine.AddressableAssets;

[assembly: RegisterValidator(typeof(DisallowAddressableSubAssetFieldAttributeValidator))]

namespace Plugins.Sirenix.Odin_Inspector.Modules.Unity.Addressables.Validators
{
	/// <summary>
	/// Validator for the DisallowAddressableSubAssetFieldAttribute.
	/// </summary>
	public class DisallowAddressableSubAssetFieldAttributeValidator : AttributeValidator<DisallowAddressableSubAssetFieldAttribute, AssetReference>
    {
        protected override void Validate(ValidationResult result)
        {
            if (this.Value != null && string.IsNullOrEmpty(this.Value.SubObjectName) == false)
            {
                result.AddError("Sub-asset references is not allowed on this field.")
                    .WithFix("Remove Sub-Asset", () => this.Value.SubObjectName = null, true);
            }
        }
    }

}

#endif
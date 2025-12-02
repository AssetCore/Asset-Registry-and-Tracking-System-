using FluentValidation;
using MaintenanceScheduler.Application.DTOs;
using System;

namespace MaintenanceScheduler.Application.Validators
{
    public class CreateWarrantyInfoValidator : AbstractValidator<CreateWarrantyInfoDto>
    {
        public CreateWarrantyInfoValidator()
        {
            RuleFor(x => x.AssetId)
                .NotEmpty().WithMessage("Asset ID is required");

            RuleFor(x => x.AssetName)
                .NotEmpty().WithMessage("Asset name is required")
                .MaximumLength(200).WithMessage("Asset name cannot exceed 200 characters");

            RuleFor(x => x.WarrantyProvider)
                .NotEmpty().WithMessage("Warranty provider is required")
                .MaximumLength(200).WithMessage("Warranty provider cannot exceed 200 characters");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Expiry date must be after start date");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
                .WithMessage("Invalid email format");

            RuleFor(x => x.WarrantyCost)
                .GreaterThanOrEqualTo(0).WithMessage("Warranty cost cannot be negative");
        }
    }
}
using FluentValidation;
using MaintenanceScheduler.Application.DTOs;
using System;

namespace MaintenanceScheduler.Application.Validators
{
    public class CreateMaintenanceScheduleValidator : AbstractValidator<CreateMaintenanceScheduleDto>
    {
        public CreateMaintenanceScheduleValidator()
        {
            RuleFor(x => x.AssetId)
                .NotEmpty().WithMessage("Asset ID is required");

            RuleFor(x => x.AssetName)
                .NotEmpty().WithMessage("Asset name is required")
                .MaximumLength(200).WithMessage("Asset name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.ScheduledDate)
                .NotEmpty().WithMessage("Scheduled date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Scheduled date cannot be in the past");

            RuleFor(x => x.FrequencyInDays)
                .GreaterThan(0).WithMessage("Frequency must be greater than 0");

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue)
                .WithMessage("Estimated cost cannot be negative");
        }
    }
}
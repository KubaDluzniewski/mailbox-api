using Application.DTOs;
using FluentValidation;

namespace Api.Validation
{
    public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
    {
        public SendMessageDtoValidator()
        {
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Recipients).NotEmpty().WithMessage("At least one recipient is required.");
        }
    }
}

﻿using Model;

namespace WebApi.Model
{
    public static class AddressExtensions
    {
        public static MaskedEmail ToModel(this Data.Model.Address address)
        {
            return new MaskedEmail
            {
                Name = address.Name,
                Description = address.Description,
                EmailAddress = address.EmailAddress,
                ForwardToEmailAddress = address.EnableForwarding ? address.Profile.ForwardingAddress : null,

                Received = address.Received,
                CreatedUtc = address.CreatedUtc,
            };
        }
    }
}
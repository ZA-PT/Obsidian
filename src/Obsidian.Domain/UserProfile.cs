﻿using System.Security.Claims;

namespace Obsidian.Domain
{
    public class UserProfile
    {
        /// <summary>
        /// Represents the first name of a <see cref="User"/>.
        /// </summary>
        [ClaimType(ClaimTypes.GivenName)]
        public string GivenName { get; set; }

        /// <summary>
        /// Represents the last name of a <see cref="User"/>.
        /// </summary>
        [ClaimType(ClaimTypes.Surname)]
        public string SurnName { get; set; }

        /// <summary>
        /// Represents the Gender of a <see cref="User"/>.
        /// </summary>
        [ClaimType(ClaimTypes.Gender)]
        public Gender Gender { get; set; }

        /// <summary>
        /// Represents the Email address of a <see cref="User"/>.
        /// </summary>
        [ClaimType(ClaimTypes.Email)]
        public string EmailAddress { get; set; }

    }
}

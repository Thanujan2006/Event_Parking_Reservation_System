namespace Event_Parking_Reservation_System.Dtos.CustomerDtos
{
    public class CustomerDtos
    {
        /// <summary>Request payload for POST /api/customers/register (BRD 4.1.5 / AC1, AC2)</summary>
        public class RegisterCustomerRequest
        {
            public string Name { get; set; } = string.Empty;

            /// <summary>SPA alias for Name (register.js sends fullName).</summary>
            public string FullName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;

            /// <summary>SPA alias for PhoneNumber (register.js sends phone).</summary>
            public string Phone { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;

            public string ResolvedName =>
                !string.IsNullOrWhiteSpace(Name) ? Name : FullName;

            public string ResolvedPhone =>
                !string.IsNullOrWhiteSpace(PhoneNumber) ? PhoneNumber : Phone;
        }

        /// <summary>Response returned after successful registration (201 Created).</summary>
        public class RegisterCustomerResponse
        {
            public int CustomerId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Message { get; set; } = "Registration successful. You can log in now.";
        }

        /// <summary>Customer's own profile view (GET /api/customers/me).</summary>
        public class CustomerProfileDto
        {
            public int CustomerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        /// <summary>
        /// Update payload for PUT /api/customers/me.
        /// Note: Email is intentionally excluded — BRD 4.1.12 forbids changing
        /// email via profile update to avoid breaking the verification flow.
        /// </summary>
        public class UpdateCustomerProfileRequest
        {
            public string Name { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
        }

        /// <summary>Admin search/filter result row (GET /api/admin/customers?search=).</summary>
        public class CustomerSummaryDto
        {
            public int CustomerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        /// <summary>Full admin detail view, including booking summary (BRD 4.1.2).</summary>
        public class CustomerDetailDto
        {
            public int CustomerId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime? DeactivatedAt { get; set; }
            public int TotalBookings { get; set; }
            public int ActiveFutureBookings { get; set; }
        }
    }
}

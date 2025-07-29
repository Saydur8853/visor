-- SQL INSERT query to add invitation for saydur776@gmail.com
-- Make sure to replace the InvitedByUserId with an actual user ID from your Users table

-- First, check existing users to get a valid InvitedByUserId
-- SELECT Id, FullName, Email FROM Users LIMIT 5;

-- Insert the invitation record
-- Note: You'll need to replace '1' in InvitedByUserId with an actual user ID from your Users table
INSERT INTO Invitations (
    Email,
    PhoneNumber,
    InvitedByUserId,
    RoleId,
    IsUsed,
    ExpiresAt,
    CreatedAt,
    UsedAt
) VALUES (
    'saydur776@gmail.com',           -- Email (required)
    NULL,                            -- PhoneNumber (optional, can be NULL)
    1,                               -- InvitedByUserId (REPLACE with actual user ID)
    2,                               -- RoleId (2 = Normal User, 1 = Super Admin, 3 = Property Admin, 4 = Security Admin)
    0,                               -- IsUsed (false/0)
    DATE_ADD(UTC_TIMESTAMP(), INTERVAL 30 DAY), -- ExpiresAt (30 days from now)
    UTC_TIMESTAMP(),                 -- CreatedAt (current UTC time)
    NULL                             -- UsedAt (NULL since not used yet)
);

-- Query to verify the insertion
-- SELECT * FROM Invitations WHERE Email = 'saydur776@gmail.com';

-- Available Roles:
-- 1 = Super Admin
-- 2 = Normal User  
-- 3 = Property Admin
-- 4 = Security Admin

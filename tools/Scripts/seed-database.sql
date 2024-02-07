DO $$
   
DECLARE user_name CONSTANT VARCHAR(128) := 'mail@mail.com';
DECLARE password_hash CONSTANT VARCHAR(255) := 'AQAAAAIAAYagAAAAEMbO4U0+72DTJTrynS9mX+9SokWZbBi9opL+zLvvgauBIWs6QRwDpUFPaHdEe6e+Mw==';

BEGIN
    
INSERT INTO identity.user 
(id, user_name, normalized_user_name, email, normalized_email, password_hash, access_failed_count, concurrency_stamp,
 security_stamp, lockout_enabled, two_factor_enabled, phone_number_confirmed, email_confirmed)
VALUES (gen_random_uuid(), user_name, upper(user_name), user_name, upper(user_name), password_hash, 0, gen_random_uuid()::text,
       gen_random_uuid()::text, false, false, false, false);

END $$
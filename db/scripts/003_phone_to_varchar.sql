ALTER TABLE users
    ALTER COLUMN phone TYPE varchar(30) USING phone::text;

ALTER TABLE users
    ALTER COLUMN phone_extra TYPE varchar(30) USING phone_extra::text;

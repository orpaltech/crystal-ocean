CREATE SEQUENCE public."User_Id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

ALTER SEQUENCE public."User_Id_seq"
    OWNER TO orpal;

CREATE TABLE public."User"
(
    "Id" integer NOT NULL DEFAULT nextval('"User_Id_seq"'::regclass),
    "UpdateTime" timestamp without time zone,
    "UserName" varchar(100) NOT NULL,
    "NormalizedUserName" varchar(100),
    "PasswordHash" varchar(100),
    "FirstName" varchar(100),
    "LastName" varchar(100),
    "MiddleName" varchar(100),
    "Prefix" varchar(100),
    "Gender" varchar(20),
    "PhoneNumber" varchar(20),
    "PhoneNumberConfirmed" boolean NOT NULL DEFAULT false,
    "Email" varchar(100),
    "NormalizedEmail" varchar(100) NOT NULL,
    "EmailConfirmed" boolean NOT NULL DEFAULT false,
    "LockoutEnabled" boolean NOT NULL DEFAULT true,
    "LockoutEnd" timestamp without time zone,
    "SecurityStamp" varchar(250),
    "ConcurrencyStamp" varchar(250),
    "TwoFactorEnabled" boolean NOT NULL DEFAULT true,
    "AccessFailedCount" integer NOT NULL DEFAULT 0,
    CONSTRAINT user_pkey PRIMARY KEY ("Id")
        USING INDEX TABLESPACE crystaloceanindexes
)
TABLESPACE crystaloceanspace;

ALTER TABLE public."User"
    OWNER to orpal;

COMMENT ON COLUMN public."User"."SecurityStamp"
    IS 'A random value that must change whenever a users credentials change (password changed, login removed)';

COMMENT ON COLUMN public."User"."PhoneNumberConfirmed"
    IS 'Flag indicating if a user has confirmed their telephone address';

COMMENT ON COLUMN public."User"."EmailConfirmed"
    IS 'Flag indicating if a user has confirmed their email address';

COMMENT ON COLUMN public."User"."ConcurrencyStamp"
    IS 'A random value that must change whenever a user is persisted to the store';

COMMENT ON COLUMN public."User"."NormalizedEmail"
    IS 'The normalized email address for this user';

COMMENT ON COLUMN public."User"."TwoFactorEnabled"
    IS 'Flag indicating if two factor authentication is enabled for this user';

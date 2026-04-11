-- Pawtrak Initial Schema
-- Migration 001 — run via DbUp on startup

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ─────────────────────────────────────────
-- Owners
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Owners" (
    "Id"               UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "FirstName"        VARCHAR(100) NOT NULL,
    "LastName"         VARCHAR(100) NOT NULL,
    "Email"            VARCHAR(255) NOT NULL UNIQUE,
    "PhoneNumber"      VARCHAR(20)  NOT NULL,
    "Address"          VARCHAR(255) NOT NULL,
    "City"             VARCHAR(100) NOT NULL,
    "State"            VARCHAR(50)  NOT NULL,
    "ZipCode"          VARCHAR(10)  NOT NULL,
    "ProfilePhotoUrl"  TEXT,
    "StripeCustomerId" VARCHAR(255),
    "CreatedAt"        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "UpdatedAt"        TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Walkers
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Walkers" (
    "Id"                  UUID           NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "FirstName"           VARCHAR(100)   NOT NULL,
    "LastName"            VARCHAR(100)   NOT NULL,
    "Email"               VARCHAR(255)   NOT NULL UNIQUE,
    "PhoneNumber"         VARCHAR(20)    NOT NULL,
    "Bio"                 TEXT           NOT NULL DEFAULT '',
    "City"                VARCHAR(100)   NOT NULL,
    "State"               VARCHAR(50)    NOT NULL,
    "ZipCode"             VARCHAR(10)    NOT NULL,
    "ProfilePhotoUrl"     TEXT,
    "HourlyRate"          NUMERIC(8,2)   NOT NULL DEFAULT 0,
    "IsAvailable"         BOOLEAN        NOT NULL DEFAULT FALSE,
    "IsBackgroundChecked" BOOLEAN        NOT NULL DEFAULT FALSE,
    "AverageRating"       DOUBLE PRECISION NOT NULL DEFAULT 0,
    "TotalReviews"        INTEGER        NOT NULL DEFAULT 0,
    "StripeAccountId"     VARCHAR(255),
    "CreatedAt"           TIMESTAMPTZ    NOT NULL DEFAULT NOW(),
    "UpdatedAt"           TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Walker Availability
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "WalkerAvailabilities" (
    "Id"          UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "WalkerId"    UUID        NOT NULL REFERENCES "Walkers"("Id") ON DELETE CASCADE,
    "DayOfWeek"   SMALLINT    NOT NULL, -- 0=Sunday … 6=Saturday
    "StartTime"   TIME        NOT NULL,
    "EndTime"     TIME        NOT NULL,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Dogs
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Dogs" (
    "Id"               UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "OwnerId"          UUID         NOT NULL REFERENCES "Owners"("Id") ON DELETE CASCADE,
    "Name"             VARCHAR(100) NOT NULL,
    "Breed"            VARCHAR(100) NOT NULL,
    "AgeYears"         INTEGER      NOT NULL DEFAULT 0,
    "WeightLbs"        NUMERIC(6,2) NOT NULL DEFAULT 0,
    "PhotoUrl"         TEXT,
    "MedicalNotes"     TEXT,
    "BehaviorNotes"    TEXT,
    "EmergencyContact" VARCHAR(255),
    "VetName"          VARCHAR(255),
    "VetPhone"         VARCHAR(20),
    "CreatedAt"        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "UpdatedAt"        TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Routine Tasks (per-dog checklist)
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "RoutineTasks" (
    "Id"          UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "DogId"       UUID         NOT NULL REFERENCES "Dogs"("Id") ON DELETE CASCADE,
    "Title"       VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "SortOrder"   INTEGER      NOT NULL DEFAULT 0,
    "IsRequired"  BOOLEAN      NOT NULL DEFAULT TRUE,
    "Category"    SMALLINT     NOT NULL DEFAULT 0,
    "CreatedAt"   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Household Members
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "HouseholdMembers" (
    "Id"                  UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "OwnerId"             UUID         NOT NULL REFERENCES "Owners"("Id") ON DELETE CASCADE,
    "Name"                VARCHAR(200) NOT NULL,
    "Email"               VARCHAR(255) NOT NULL,
    "PhoneNumber"         VARCHAR(20)  NOT NULL,
    "NotifyOnBooking"     BOOLEAN      NOT NULL DEFAULT TRUE,
    "NotifyOnVisitStart"  BOOLEAN      NOT NULL DEFAULT TRUE,
    "NotifyOnVisitEnd"    BOOLEAN      NOT NULL DEFAULT TRUE,
    "CreatedAt"           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "UpdatedAt"           TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Bookings
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Bookings" (
    "Id"                   UUID          NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "OwnerId"              UUID          NOT NULL REFERENCES "Owners"("Id"),
    "WalkerId"             UUID          NOT NULL REFERENCES "Walkers"("Id"),
    "DogId"                UUID          NOT NULL REFERENCES "Dogs"("Id"),
    "ScheduledStart"       TIMESTAMPTZ   NOT NULL,
    "ScheduledEnd"         TIMESTAMPTZ   NOT NULL,
    "ActualStart"          TIMESTAMPTZ,
    "ActualEnd"            TIMESTAMPTZ,
    "Status"               SMALLINT      NOT NULL DEFAULT 0,
    "BaseAmount"           NUMERIC(10,2) NOT NULL DEFAULT 0,
    "PlatformFee"          NUMERIC(10,2) NOT NULL DEFAULT 0,
    "WalkerEarnings"       NUMERIC(10,2) NOT NULL DEFAULT 0,
    "TipAmount"            NUMERIC(10,2),
    "SpecialInstructions"  TEXT,
    "StripePaymentIntentId" VARCHAR(255),
    "CreatedAt"            TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "UpdatedAt"            TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "ix_bookings_owner"  ON "Bookings"("OwnerId");
CREATE INDEX IF NOT EXISTS "ix_bookings_walker" ON "Bookings"("WalkerId");
CREATE INDEX IF NOT EXISTS "ix_bookings_status" ON "Bookings"("Status");

-- ─────────────────────────────────────────
-- Visit Reports
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "VisitReports" (
    "Id"          UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "BookingId"   UUID        NOT NULL UNIQUE REFERENCES "Bookings"("Id") ON DELETE CASCADE,
    "Notes"       TEXT,
    "Status"      SMALLINT    NOT NULL DEFAULT 0,
    "CompletedAt" TIMESTAMPTZ,
    "CreatedAt"   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"   TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Visit Task Logs
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "VisitTaskLogs" (
    "Id"              UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "VisitReportId"   UUID        NOT NULL REFERENCES "VisitReports"("Id") ON DELETE CASCADE,
    "RoutineTaskId"   UUID        NOT NULL REFERENCES "RoutineTasks"("Id"),
    "IsCompleted"     BOOLEAN     NOT NULL DEFAULT FALSE,
    "CompletedAt"     TIMESTAMPTZ,
    "Notes"           TEXT,
    "CreatedAt"       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"       TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Visit Photos
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "VisitPhotos" (
    "Id"            UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "VisitReportId" UUID        NOT NULL REFERENCES "VisitReports"("Id") ON DELETE CASCADE,
    "PhotoUrl"      TEXT        NOT NULL,
    "Type"          SMALLINT    NOT NULL DEFAULT 0,  -- 0=Before, 1=During, 2=After
    "Caption"       TEXT,
    "TakenAt"       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "CreatedAt"     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt"     TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- GPS Points
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "GpsPoints" (
    "Id"            UUID             NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "VisitReportId" UUID             NOT NULL REFERENCES "VisitReports"("Id") ON DELETE CASCADE,
    "Latitude"      DOUBLE PRECISION NOT NULL,
    "Longitude"     DOUBLE PRECISION NOT NULL,
    "Accuracy"      DOUBLE PRECISION,
    "RecordedAt"    TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    "CreatedAt"     TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    "UpdatedAt"     TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "ix_gpspoints_visit" ON "GpsPoints"("VisitReportId", "RecordedAt");

-- ─────────────────────────────────────────
-- Payments
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Payments" (
    "Id"                   UUID          NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "BookingId"            UUID          NOT NULL UNIQUE REFERENCES "Bookings"("Id"),
    "Amount"               NUMERIC(10,2) NOT NULL,
    "PlatformFee"          NUMERIC(10,2) NOT NULL,
    "WalkerPayout"         NUMERIC(10,2) NOT NULL,
    "TipAmount"            NUMERIC(10,2),
    "Status"               SMALLINT      NOT NULL DEFAULT 0,
    "StripePaymentIntentId" VARCHAR(255) NOT NULL,
    "StripeChargeId"       VARCHAR(255),
    "StripeTransferId"     VARCHAR(255),
    "AuthorizedAt"         TIMESTAMPTZ,
    "ReleasedAt"           TIMESTAMPTZ,
    "CreatedAt"            TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "UpdatedAt"            TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- Reviews
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id"        UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    "BookingId" UUID        NOT NULL UNIQUE REFERENCES "Bookings"("Id"),
    "OwnerId"   UUID        NOT NULL REFERENCES "Owners"("Id"),
    "WalkerId"  UUID        NOT NULL REFERENCES "Walkers"("Id"),
    "Rating"    SMALLINT    NOT NULL CHECK ("Rating" BETWEEN 1 AND 5),
    "Comment"   TEXT,
    "IsPublic"  BOOLEAN     NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "ix_reviews_walker" ON "Reviews"("WalkerId");

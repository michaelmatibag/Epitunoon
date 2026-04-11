-- Pawtrak Initial Schema
-- Migration 001 — run via DbUp on startup
-- All identifiers are lowercase snake_case; no quoting needed in queries.
-- Dapper maps snake_case columns → PascalCase properties via MatchNamesWithUnderscores.

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ─────────────────────────────────────────
-- owners
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS owners (
    id                UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    first_name        VARCHAR(100) NOT NULL,
    last_name         VARCHAR(100) NOT NULL,
    email             VARCHAR(255) NOT NULL UNIQUE,
    phone_number      VARCHAR(20)  NOT NULL,
    address           VARCHAR(255) NOT NULL,
    city              VARCHAR(100) NOT NULL,
    state             VARCHAR(50)  NOT NULL,
    zip_code          VARCHAR(10)  NOT NULL,
    profile_photo_url TEXT,
    stripe_customer_id VARCHAR(255),
    created_at        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- walkers
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS walkers (
    id                   UUID             NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    first_name           VARCHAR(100)     NOT NULL,
    last_name            VARCHAR(100)     NOT NULL,
    email                VARCHAR(255)     NOT NULL UNIQUE,
    phone_number         VARCHAR(20)      NOT NULL,
    bio                  TEXT             NOT NULL DEFAULT '',
    city                 VARCHAR(100)     NOT NULL,
    state                VARCHAR(50)      NOT NULL,
    zip_code             VARCHAR(10)      NOT NULL,
    profile_photo_url    TEXT,
    hourly_rate          NUMERIC(8,2)     NOT NULL DEFAULT 0,
    is_available         BOOLEAN          NOT NULL DEFAULT FALSE,
    is_background_checked BOOLEAN         NOT NULL DEFAULT FALSE,
    average_rating       DOUBLE PRECISION NOT NULL DEFAULT 0,
    total_reviews        INTEGER          NOT NULL DEFAULT 0,
    stripe_account_id    VARCHAR(255),
    created_at           TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- walker_availabilities
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS walker_availabilities (
    id          UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    walker_id   UUID        NOT NULL REFERENCES walkers(id) ON DELETE CASCADE,
    day_of_week SMALLINT    NOT NULL, -- 0=Sunday … 6=Saturday
    start_time  TIME        NOT NULL,
    end_time    TIME        NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- dogs
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS dogs (
    id                UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    owner_id          UUID         NOT NULL REFERENCES owners(id) ON DELETE CASCADE,
    name              VARCHAR(100) NOT NULL,
    breed             VARCHAR(100) NOT NULL,
    age_years         INTEGER      NOT NULL DEFAULT 0,
    weight_lbs        NUMERIC(6,2) NOT NULL DEFAULT 0,
    photo_url         TEXT,
    medical_notes     TEXT,
    behavior_notes    TEXT,
    emergency_contact VARCHAR(255),
    vet_name          VARCHAR(255),
    vet_phone         VARCHAR(20),
    created_at        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- routine_tasks  (per-dog ordered checklist)
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS routine_tasks (
    id          UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    dog_id      UUID         NOT NULL REFERENCES dogs(id) ON DELETE CASCADE,
    title       VARCHAR(200) NOT NULL,
    description TEXT,
    sort_order  INTEGER      NOT NULL DEFAULT 0,
    is_required BOOLEAN      NOT NULL DEFAULT TRUE,
    category    SMALLINT     NOT NULL DEFAULT 0,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- household_members
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS household_members (
    id                   UUID         NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    owner_id             UUID         NOT NULL REFERENCES owners(id) ON DELETE CASCADE,
    name                 VARCHAR(200) NOT NULL,
    email                VARCHAR(255) NOT NULL,
    phone_number         VARCHAR(20)  NOT NULL,
    notify_on_booking    BOOLEAN      NOT NULL DEFAULT TRUE,
    notify_on_visit_start BOOLEAN     NOT NULL DEFAULT TRUE,
    notify_on_visit_end  BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- bookings
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS bookings (
    id                     UUID          NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    owner_id               UUID          NOT NULL REFERENCES owners(id),
    walker_id              UUID          NOT NULL REFERENCES walkers(id),
    dog_id                 UUID          NOT NULL REFERENCES dogs(id),
    scheduled_start        TIMESTAMPTZ   NOT NULL,
    scheduled_end          TIMESTAMPTZ   NOT NULL,
    actual_start           TIMESTAMPTZ,
    actual_end             TIMESTAMPTZ,
    status                 SMALLINT      NOT NULL DEFAULT 0,
    base_amount            NUMERIC(10,2) NOT NULL DEFAULT 0,
    platform_fee           NUMERIC(10,2) NOT NULL DEFAULT 0,
    walker_earnings        NUMERIC(10,2) NOT NULL DEFAULT 0,
    tip_amount             NUMERIC(10,2),
    special_instructions   TEXT,
    stripe_payment_intent_id VARCHAR(255),
    created_at             TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_bookings_owner  ON bookings(owner_id);
CREATE INDEX IF NOT EXISTS ix_bookings_walker ON bookings(walker_id);
CREATE INDEX IF NOT EXISTS ix_bookings_status ON bookings(status);

-- ─────────────────────────────────────────
-- visit_reports
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS visit_reports (
    id           UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    booking_id   UUID        NOT NULL UNIQUE REFERENCES bookings(id) ON DELETE CASCADE,
    notes        TEXT,
    status       SMALLINT    NOT NULL DEFAULT 0,
    completed_at TIMESTAMPTZ,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- visit_task_logs
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS visit_task_logs (
    id              UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    visit_report_id UUID        NOT NULL REFERENCES visit_reports(id) ON DELETE CASCADE,
    routine_task_id UUID        NOT NULL REFERENCES routine_tasks(id),
    is_completed    BOOLEAN     NOT NULL DEFAULT FALSE,
    completed_at    TIMESTAMPTZ,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- visit_photos
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS visit_photos (
    id              UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    visit_report_id UUID        NOT NULL REFERENCES visit_reports(id) ON DELETE CASCADE,
    photo_url       TEXT        NOT NULL,
    type            SMALLINT    NOT NULL DEFAULT 0,  -- 0=Before 1=During 2=After
    caption         TEXT,
    taken_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- gps_points
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS gps_points (
    id              UUID             NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    visit_report_id UUID             NOT NULL REFERENCES visit_reports(id) ON DELETE CASCADE,
    latitude        DOUBLE PRECISION NOT NULL,
    longitude       DOUBLE PRECISION NOT NULL,
    accuracy        DOUBLE PRECISION,
    recorded_at     TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    created_at      TIMESTAMPTZ      NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_gps_points_visit ON gps_points(visit_report_id, recorded_at);

-- ─────────────────────────────────────────
-- payments
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS payments (
    id                       UUID          NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    booking_id               UUID          NOT NULL UNIQUE REFERENCES bookings(id),
    amount                   NUMERIC(10,2) NOT NULL,
    platform_fee             NUMERIC(10,2) NOT NULL,
    walker_payout            NUMERIC(10,2) NOT NULL,
    tip_amount               NUMERIC(10,2),
    status                   SMALLINT      NOT NULL DEFAULT 0,
    stripe_payment_intent_id VARCHAR(255)  NOT NULL,
    stripe_charge_id         VARCHAR(255),
    stripe_transfer_id       VARCHAR(255),
    authorized_at            TIMESTAMPTZ,
    released_at              TIMESTAMPTZ,
    created_at               TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at               TIMESTAMPTZ
);

-- ─────────────────────────────────────────
-- reviews
-- ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS reviews (
    id          UUID        NOT NULL DEFAULT uuid_generate_v4() PRIMARY KEY,
    booking_id  UUID        NOT NULL UNIQUE REFERENCES bookings(id),
    owner_id    UUID        NOT NULL REFERENCES owners(id),
    walker_id   UUID        NOT NULL REFERENCES walkers(id),
    rating      SMALLINT    NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment     TEXT,
    is_public   BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_reviews_walker ON reviews(walker_id);

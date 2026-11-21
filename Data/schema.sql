-- IDs são exclusivamente internos. JSON REST usa nomes com hífen e nunca expõe ID.
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS applications (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    resource_name TEXT NOT NULL UNIQUE CHECK(length(trim(resource_name)) > 0),
    resource_desc TEXT NOT NULL DEFAULT '',
    created_at TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS channels (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    application_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL CHECK(length(trim(resource_name)) > 0),
    resource_desc TEXT NOT NULL DEFAULT '',
    created_at TEXT NOT NULL,
    FOREIGN KEY (application_id) REFERENCES applications(id) ON DELETE CASCADE,
    UNIQUE (application_id, resource_name)
);
CREATE TABLE IF NOT EXISTS data_records (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    channel_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL CHECK(length(trim(resource_name)) > 0 AND lower(resource_name) <> 'subs'),
    content_type TEXT NOT NULL,
    content TEXT NOT NULL,
    created_at TEXT NOT NULL,
    expires_at TEXT NULL,
    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
    UNIQUE (channel_id, resource_name)
);
CREATE TABLE IF NOT EXISTS subscriptions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    channel_id INTEGER NOT NULL,
    resource_name TEXT NOT NULL CHECK(length(trim(resource_name)) > 0),
    evt INTEGER NOT NULL CHECK (evt IN (1, 2)),
    endpoint TEXT NOT NULL CHECK(length(trim(endpoint)) > 0),
    created_at TEXT NOT NULL,
    expires_at TEXT NULL,
    FOREIGN KEY (channel_id) REFERENCES channels(id) ON DELETE CASCADE,
    UNIQUE (channel_id, resource_name)
);
CREATE INDEX IF NOT EXISTS ix_data_records_expiration ON data_records(expires_at);
CREATE INDEX IF NOT EXISTS ix_subscriptions_expiration ON subscriptions(expires_at);
CREATE INDEX IF NOT EXISTS ix_subscriptions_channel_event ON subscriptions(channel_id, evt);

-- [PROPOSTA Q-03] No mesmo channel nomes de records e subscriptions não se repetem.
-- Validação ao nível da BD, além da geração de nome única no serviço público.
CREATE TRIGGER IF NOT EXISTS tr_data_records_cross_name_insert
BEFORE INSERT ON data_records
WHEN EXISTS (
    SELECT 1 FROM subscriptions s
    WHERE s.channel_id = NEW.channel_id AND s.resource_name = NEW.resource_name
)
BEGIN
    SELECT RAISE(ABORT, 'resource-name already used in channel');
END;

CREATE TRIGGER IF NOT EXISTS tr_subscriptions_cross_name_insert
BEFORE INSERT ON subscriptions
WHEN EXISTS (
    SELECT 1 FROM data_records r
    WHERE r.channel_id = NEW.channel_id AND r.resource_name = NEW.resource_name
)
BEGIN
    SELECT RAISE(ABORT, 'resource-name already used in channel');
END;

CREATE TRIGGER IF NOT EXISTS tr_data_records_cross_name_update
BEFORE UPDATE OF resource_name, channel_id ON data_records
WHEN EXISTS (
    SELECT 1 FROM subscriptions s
    WHERE s.channel_id = NEW.channel_id AND s.resource_name = NEW.resource_name
)
BEGIN
    SELECT RAISE(ABORT, 'resource-name already used in channel');
END;

CREATE TRIGGER IF NOT EXISTS tr_subscriptions_cross_name_update
BEFORE UPDATE OF resource_name, channel_id ON subscriptions
WHEN EXISTS (
    SELECT 1 FROM data_records r
    WHERE r.channel_id = NEW.channel_id AND r.resource_name = NEW.resource_name
)
BEGIN
    SELECT RAISE(ABORT, 'resource-name already used in channel');
END;

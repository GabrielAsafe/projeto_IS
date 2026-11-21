-- SAMPLE DATA: demonstrativo de modelação, não base de dados de implementação pronta.
-- Recriar com schema.sql antes de executar seed.sql.
PRAGMA foreign_keys = ON;
INSERT INTO applications(resource_name,resource_desc,created_at)
VALUES('app1','Application required by assignment',datetime('now'));

INSERT INTO channels(application_id,resource_name,resource_desc,created_at)
VALUES((SELECT id FROM applications WHERE resource_name='app1'),'channel1','Channel required by assignment',datetime('now'));

INSERT INTO data_records(channel_id,resource_name,content_type,content,created_at,expires_at)
VALUES((SELECT ch.id FROM channels ch 
        JOIN applications app ON ch.application_id=app.id 
        WHERE app.resource_name='app1' AND ch.resource_name='channel1'),
        'dr1','application/json','{"humidity":45}',datetime('now'),NULL);


--subscrição para testar criações 
INSERT INTO subscriptions(channel_id,resource_name,evt,endpoint,created_at,expires_at)
VALUES((SELECT ch.id FROM channels ch 
        JOIN applications app ON ch.application_id=app.id 
        WHERE app.resource_name='app1' AND ch.resource_name='channel1'),
        'sub1',1,'http://localhost:9090/notifications',datetime('now'),NULL);



--subscrição para testar expiração
INSERT INTO subscriptions(
    channel_id,
    resource_name,
    evt,
    endpoint,
    created_at,
    expires_at
)
SELECT
    ch.id,
    'sub-delete',
    2,
    'http://localhost:9090/notifications',
    strftime('%Y-%m-%dT%H:%M:%S', 'now'),
    NULL
FROM channels ch
JOIN applications app
    ON ch.application_id = app.id
WHERE app.resource_name = 'app1'
  AND ch.resource_name = 'channel1';




INSERT INTO data_records(
    channel_id,
    resource_name,
    content_type,
    content,
    created_at,
    expires_at
)
SELECT
    ch.id,
    'dr-expiration-test',
    'text/plain',
    '25',
    strftime('%Y-%m-%dT%H:%M:%S', 'now'),
    strftime('%Y-%m-%dT%H:%M:%S', 'now', '+20 seconds')
FROM channels ch
JOIN applications app
    ON ch.application_id = app.id
WHERE app.resource_name = 'app1'
  AND ch.resource_name = 'channel1';


  --subscrição para testar criações 
INSERT INTO subscriptions(channel_id,resource_name,evt,endpoint,created_at,expires_at)
VALUES((SELECT ch.id FROM channels ch 
        JOIN applications app ON ch.application_id=app.id 
        WHERE app.resource_name='app1' AND ch.resource_name='channel1'),
        'sub2',2,'http://localhost:9090/notifications', 
        strftime('%Y-%m-%dT%H:%M:%S', 'now'),
    strftime('%Y-%m-%dT%H:%M:%S', 'now', '+20 seconds'));

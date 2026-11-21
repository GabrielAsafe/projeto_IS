"""Testes do protótipo SQLite SOLID v2. Não testam API .NET/HTTP/MQTT."""
import sqlite3
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCHEMA = (ROOT / 'sql/schema.sql').read_text(encoding='utf8')
SEED = (ROOT / 'sql/seed.sql').read_text(encoding='utf8')

class ModelSqliteTests(unittest.TestCase):
    def setUp(self):
        self.db = sqlite3.connect(':memory:')
        self.db.execute('PRAGMA foreign_keys=ON')
        self.db.executescript(SCHEMA)
        self.db.executescript(SEED)
        self.db.commit()
        self.channel_id = self.db.execute('SELECT id FROM channels WHERE resource_name=?', ('channel1',)).fetchone()[0]

    def tearDown(self):
        self.db.close()

    def test_seed_has_required_resources(self):
        for table, name in [('applications','app1'),('channels','channel1'),('data_records','dr1'),('subscriptions','sub1')]:
            self.assertEqual(1, self.db.execute(f'SELECT COUNT(*) FROM {table} WHERE resource_name=?', (name,)).fetchone()[0])

    def test_subscription_evt_only_one_or_two(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO subscriptions(channel_id,resource_name,evt,endpoint,created_at) VALUES(?, 'bad',3,'http://localhost:1','2026-10-27T09:00:00')", (self.channel_id,))

    def test_duplicate_name_across_record_and_sub_is_rejected(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO subscriptions(channel_id,resource_name,evt,endpoint,created_at) VALUES(?, 'dr1',1,'http://localhost:1','2026-10-27T09:00:00')", (self.channel_id,))
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO data_records(channel_id,resource_name,content_type,content,created_at) VALUES(?, 'sub1','text/plain','hello','2026-10-27T09:00:00')", (self.channel_id,))

    def test_record_name_subs_is_reserved(self):
        with self.assertRaises(sqlite3.IntegrityError):
            self.db.execute("INSERT INTO data_records(channel_id,resource_name,content_type,content,created_at) VALUES(?, 'subs','text/plain','hello','2026-10-27T09:00:00')", (self.channel_id,))

    def test_same_channel_name_allowed_in_different_app(self):
        self.db.execute("INSERT INTO applications(resource_name,resource_desc,created_at) VALUES('app2','','2026-10-27T09:00:00')")
        aid=self.db.execute("SELECT id FROM applications WHERE resource_name='app2'").fetchone()[0]
        self.db.execute('INSERT INTO channels(application_id,resource_name,resource_desc,created_at) VALUES(?,?,?,?)',(aid,'channel1','','2026-10-27T09:00:00'))

    def test_deleting_parent_cascades_children(self):
        self.db.execute("DELETE FROM applications WHERE resource_name='app1'")
        for t in ['channels','data_records','subscriptions']:
            self.assertEqual(0,self.db.execute(f'SELECT COUNT(*) FROM {t}').fetchone()[0])

if __name__ == '__main__':
    unittest.main()

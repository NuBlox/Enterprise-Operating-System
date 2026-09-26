'use strict';

var assert = require('assert');
var mysql = require('../../promise');

var config = {
  host                    : process.env.MYSQL_HOST || '127.0.0.1',
  port                    : Number(process.env.MYSQL_PORT || 3306),
  user                    : process.env.MYSQL_USER || 'nublox',
  password                : process.env.MYSQL_PASSWORD || 'nublox_ci_password',
  database                : process.env.MYSQL_DATABASE || 'nublox_ci',
  allowPublicKeyRetrieval : true
};

async function run() {
  var connection = mysql.createConnection(config);

  await connection.connect();

  var queryResult = await connection.query('SELECT 1 AS value, CURRENT_USER() AS currentUser, VERSION() AS version');
  var rows = queryResult[0];

  assert.strictEqual(rows.length, 1);
  assert.strictEqual(rows[0].value, 1);
  assert.ok(rows[0].currentUser);
  assert.ok(rows[0].version);

  await connection.end();

  var pool = mysql.createPool(Object.assign({}, config, {
    connectionLimit: 2
  }));
  var health = await pool.healthCheck();

  assert.strictEqual(health.ok, true);

  var value = await pool.withTransaction(async function (transaction) {
    var result = await transaction.query('SELECT 2 AS value');
    return result[0][0].value;
  });

  assert.strictEqual(value, 2);
  await pool.end();
}

run().catch(function (err) {
  process.nextTick(function () {
    throw err;
  });
});

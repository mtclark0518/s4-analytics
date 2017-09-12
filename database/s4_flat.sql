/*
EXECUTE AS SYSDBA
*/

-- USER SQL
CREATE USER s4_flat IDENTIFIED BY crash418b
DEFAULT TABLESPACE "USERS"
TEMPORARY TABLESPACE "TEMP";

-- QUOTAS

-- ROLES
GRANT "CTXAPP" TO s4_flat ;
GRANT "CONNECT" TO s4_flat ;
GRANT "RESOURCE" TO s4_flat ;

-- SYSTEM PRIVILEGES
GRANT CREATE TRIGGER TO s4_flat ;
GRANT DEBUG CONNECT SESSION TO s4_flat ;
GRANT CREATE MATERIALIZED VIEW TO s4_flat ;
GRANT CREATE ANY INDEX TO s4_flat ;
GRANT DEBUG ANY PROCEDURE TO s4_flat ;
GRANT CREATE VIEW TO s4_flat ;
GRANT CREATE TABLE TO s4_flat ;
GRANT CREATE DATABASE LINK TO s4_flat ;
GRANT UNLIMITED TABLESPACE TO s4_flat ;

@ECHO OFF

@IF NOT (%1)==(INSTALL_SQL) GOTO RESTORE_SQL

:INSTALL_SQL
@REM install postgresql

@SET postgre_sql_instller=C:\Users\lisa3\Downloads\postgresql-9.5.3-1-windows-x64.exe 

%postgre_sql_instller% --unattendedmodeui minimal --mode unattended --superpassword "openetaxbill" --servicename "postgreSQL" --servicepassword "openetaxbill" --serverport 5432

GOTO RESTORE_SQL

:RESTORE_SQL
REM The script sets environment variables helpful for PostgreSQL

@SET PATH="C:\Program Files\PostgreSQL\9.5\bin";%PATH%
@SET PGDATA=C:\Program Files\PostgreSQL\9.5\data
@SET PGDATABASE=postgres
@SET PGUSER=postgres
@SET PGPORT=5432
@SET PGLOCALEDIR=C:\Program Files\PostgreSQL\9.5\share\locale

psql -U postgres -f init.sql template1
createdb -U odinsoft OPEN-eTAX-V10
pg_restore -U odinsoft -d OPEN-eTAX-V10 -v open-etaxbill.backup

:END_SQL
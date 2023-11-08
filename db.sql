CREATE TABLE IF NOT EXISTS app.config (
    cfg_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY,
    cfg_group varchar(255) NOT NULL,
    cfg_key varchar(255) NOT NULL,
    cfg_val varchar(255),
    cfg_int bigint,
    cfg_num double precision,
    cfg_text text,
    cfg_date timestamp,
    cfg_descr text,
    CONSTRAINT config_pkey PRIMARY KEY (cfg_id)
);

INSERT INTO app.config (cfg_group,cfg_key,cfg_val,cfg_int,cfg_num,cfg_text,cfg_date,cfg_descr) VALUES 
	('Auth','Host','login.epaslaugos.lt',null,null,null,null,'Autorizacijos portalo adresas'),	
	('Auth','Token','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9',null,null,null,null,'Autorizacijos raktas'),	
	('Auth','Redirect','http://localhost:5000/api/login',null,null,null,null,'Autorizacijos atsako adresas'),
	('Config','Reload',null,60,null,null,null,'Konfiguracijos atnaujinimo intervalas sekundėmis'),	
	('Config','DebugDB',null,1,null,null,null,'Spausdinti kodo vykdymą konsolėje');

CREATE TABLE IF NOT EXISTS app.log_error (
    log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY,
	log_date timestamp DEFAULT (now() AT TIME ZONE 'utc'::text),
    log_code bigint,
    log_msg varchar(255),
    log_data text,
    log_ip varchar(255),
    CONSTRAINT log_error_pkey PRIMARY KEY (log_id)
);

CREATE TABLE IF NOT EXISTS app.session (
    sess_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY,
	sess_key varchar(255),
    sess_data text,
    sess_expire timestamp,
    sess_extended bigint,
    CONSTRAINT session_pkey PRIMARY KEY (sess_id),
    CONSTRAINT sess_key_unique UNIQUE (sess_key)
);

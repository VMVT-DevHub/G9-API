-- Create DB:
CREATE ROLE "G9_admin"; 
CREATE DATABASE "G9" WITH OWNER = "G9_admin" ENCODING = 'UTF8' CONNECTION LIMIT = -1;

-- DB Setup:
DO LANGUAGE 'plpgsql' $$ BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_roles    WHERE rolname = 'apps'        ) THEN CREATE ROLE apps; END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_roles    WHERE rolname = 'replication' ) THEN CREATE ROLE replication WITH REPLICATION; END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_roles    WHERE rolname = 'g9_app'      ) THEN CREATE ROLE g9_app WITH LOGIN INHERIT CONNECTION LIMIT -1 PASSWORD 'g9_app'; END IF;
	IF NOT EXISTS (SELECT 1 FROM pg_roles    WHERE rolname = 'g9_repl'     ) THEN CREATE ROLE g9_repl WITH LOGIN INHERIT REPLICATION CONNECTION LIMIT -1; END IF;
	GRANT apps TO g9_app; GRANT replication TO g9_repl;
	GRANT CONNECT ON DATABASE "G9" TO g9_app;
	GRANT CONNECT ON DATABASE "G9" TO g9_repl;
	GRANT ALL ON DATABASE "G9" TO "G9_admin";
	REVOKE ALL ON DATABASE "G9" FROM PUBLIC;
END $$;

-- CONNECT TO `G9` DB


-- Schema Setup:
DO LANGUAGE 'plpgsql' $$ BEGIN
	CREATE SCHEMA IF NOT EXISTS app AUTHORIZATION "G9_admin";
	CREATE SCHEMA IF NOT EXISTS g9  AUTHORIZATION "G9_admin";
	CREATE SCHEMA IF NOT EXISTS jar AUTHORIZATION "G9_admin";
	DROP SCHEMA IF EXISTS public; 
	SET SESSION AUTHORIZATION "G9_admin"; 
	
	GRANT USAGE ON SCHEMA app TO g9_app; GRANT ALL ON SCHEMA app TO "G9_admin";
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA app GRANT ALL ON TABLES TO "G9_admin" WITH GRANT OPTION;
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA app GRANT ALL ON TABLES TO g9_app;
	GRANT USAGE ON SCHEMA g9 TO g9_app; GRANT USAGE ON SCHEMA g9 TO g9_repl; GRANT ALL ON SCHEMA g9 TO "G9_admin";
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA g9 GRANT ALL ON TABLES TO "G9_admin" WITH GRANT OPTION;
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA g9 GRANT ALL ON TABLES TO g9_app;
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA g9 GRANT SELECT ON TABLES TO g9_repl;
	GRANT USAGE ON SCHEMA jar TO g9_app; GRANT ALL ON SCHEMA jar TO "G9_admin";
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA jar GRANT ALL ON TABLES TO "G9_admin" WITH GRANT OPTION;
	ALTER DEFAULT PRIVILEGES FOR ROLE "G9_admin" IN SCHEMA jar GRANT SELECT ON TABLES TO g9_app;
END $$;



-- Table setup:
DO LANGUAGE 'plpgsql' $$ BEGIN
    SET SESSION AUTHORIZATION "G9_admin"; 
	CREATE TABLE g9.daznumas (dzn_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dzn_grupe varchar(3) NOT NULL, dzn_nuo integer NOT NULL, dzn_iki integer NOT NULL, dzn_kartai integer NOT NULL, dzn_laikas integer NOT NULL, dzn_stebesena integer, CONSTRAINT g9_daznumas_pkey PRIMARY KEY (dzn_id));
	CREATE TABLE g9.deklaravimas (dkl_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dkl_gvts bigint NOT NULL, dkl_metai integer NOT NULL, dkl_stebesena integer NOT NULL, dkl_status integer NOT NULL, dkl_kiekis double precision, dkl_vartot integer, dkl_medziagos integer[], dkl_deklar_date timestamp(3), dkl_deklar_user varchar(255), dkl_deklar_user_id uuid, dkl_modif_date timestamp(3), dkl_modif_user varchar(255), dkl_modif_user_id uuid, dkl_kontaktas_vardas varchar(255), dkl_kontaktas_pavarde varchar(255), dkl_kontaktas_email varchar(255), dkl_kontaktas_phone varchar(255), CONSTRAINT g9_deklaravimas_pkey PRIMARY KEY (dkl_id));
	CREATE TABLE g9.gvts (vkl_id bigint NOT NULL, vkl_ja bigint, vkl_title varchar(255), vkl_saviv varchar(255), vkl_adresas varchar(255), vkl_gvtot varchar(255), CONSTRAINT g9_gvts_pkey PRIMARY KEY (vkl_id));
	CREATE TABLE g9.lookup (lkp_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, lkp_group varchar(30) NOT NULL, lkp_key varchar(30), lkp_num bigint,  lkp_int integer, lkp_value varchar(255), lkp_sort integer, CONSTRAINT g9_public_lookup_pkey PRIMARY KEY (lkp_id));
	CREATE TABLE g9.rodikliai (rod_id integer NOT NULL, rod_grupe integer NOT NULL, rod_kodas varchar(30) NOT NULL, rod_rodiklis varchar(255) NOT NULL, rod_daznumas varchar(3), rod_min double precision NOT NULL, rod_max double precision NOT NULL, rod_step double precision NOT NULL, rod_vnt varchar(30) NOT NULL, rod_aprasymas text, CONSTRAINT g9_rodikliai_pkey PRIMARY KEY (rod_id));
	CREATE TABLE g9.stebesenos (stb_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, stb_stebesenos integer, stb_rodiklis integer, PRIMARY KEY (stb_id));
	CREATE TABLE g9.reiksmes (rks_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, rks_deklar integer, rks_rodiklis integer, rks_date date, rks_reiksme double precision, rks_suvedimas bigint, rks_user uuid, rks_date_add timestamp(0) DEFAULT timezone('utc', now()), CONSTRAINT reiksmes_pkey PRIMARY KEY (rks_id));
	CREATE TABLE g9.valid_trukumas (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, vld_deklar int NOT NULL, vld_rodiklis integer NOT NULL, vld_suvesta integer NOT NULL, vld_reikia integer NOT NULL,  vld_kitas boolean, vld_tvirtinti boolean DEFAULT true, vld_pastabos text, vld_user uuid, vld_date_add timestamp(0) DEFAULT timezone('utc'::text, now()), vld_date_modif timestamp(0), vld_del boolean NOT NULL DEFAULT false, CONSTRAINT valid_trukumas_pkey PRIMARY KEY (vld_id));
	CREATE TABLE g9.valid_kartojasi (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, vld_deklar int NOT NULL, vld_rodiklis integer NOT NULL, vld_data date NOT NULL, vld_reiksme double precision NOT NULL, vld_tvirtinti boolean DEFAULT true, vld_pastabos text, vld_user uuid, vld_date_add timestamp(0) DEFAULT timezone('utc'::text, now()), vld_date_modif timestamp(0), vld_del boolean NOT NULL DEFAULT false, CONSTRAINT valid_kartojasi_pkey PRIMARY KEY (vld_id));
	CREATE TABLE g9.valid_virsija (vld_id integer NOT NULL GENERATED ALWAYS AS IDENTITY,vld_deklar integer NOT NULL,vld_rodiklis integer NOT NULL,vld_nuo date NOT NULL,vld_iki date NOT NULL,vld_max double precision NOT NULL,vld_nereiksm boolean,vld_nereiksm_apras text COLLATE pg_catalog."default",vld_zmones integer,vld_tipas integer,vld_loq_verte boolean,vld_loq_reiksme double precision,vld_statusas integer,vld_priez integer,vld_veiksmas character varying(30) COLLATE pg_catalog."default",vld_pradzia date,vld_pabaiga date,vld_tvirtinti boolean DEFAULT false,vld_pastabos text COLLATE pg_catalog."default",vld_user uuid,vld_date_add timestamp(0) without time zone DEFAULT timezone('utc'::text, now()),vld_date_modif timestamp(0) without time zone,vld_del boolean NOT NULL DEFAULT false,CONSTRAINT valid_virsija_pkey PRIMARY KEY (vld_id));
	CREATE TABLE g9.suvedimai (rsv_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, rsv_deklar integer, rsv_type integer, rsv_user uuid, rsv_date timestamp(0) DEFAULT timezone('utc', now()), CONSTRAINT suvedimai_pkey PRIMARY KEY (rsv_id));
	CREATE TABLE g9.ja_detales (ja_id bigint NOT NULL, jad_kontaktas_vardas varchar(255), jad_kontaktas_pavarde varchar(255), jad_kontaktas_email varchar(255), jad_kontaktas_phone varchar(255), jad_user uuid, jad_date timestamp(0) DEFAULT timezone('utc', now()), CONSTRAINT jad_pkey PRIMARY KEY (ja_id));

	CREATE TABLE jar.data (ja_kodas int NOT NULL, ja_pavadinimas varchar(255) NOT NULL, adresas varchar(255), aob_kodas int, form_kodas int, form_pavadinimas varchar(255),status_kodas int,reg_data date, CONSTRAINT data_pkey PRIMARY KEY (ja_kodas,status_kodas));

	CREATE TABLE app.users (id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, user_id uuid NOT NULL, user_fname varchar(255) NOT NULL, user_lname varchar(255) NOT NULL, user_email varchar(255), user_phone varchar(30), user_dt_login timestamp(3),CONSTRAINT app_users_pk PRIMARY KEY (id));
	CREATE TABLE app.roles (role_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, role_gvts bigint NOT NULL, role_user uuid NOT NULL, role_admin bool NOT NULL DEFAULT false, role_date timestamp(0) NOT NULL DEFAULT timezone('utc'::text, now()), CONSTRAINT app_roles_id PRIMARY KEY (role_id)); CREATE INDEX idx_app_roles_gvts on app.roles(role_gvts); CREATE INDEX idx_app_roles_user on app.roles(role_user);
	CREATE TABLE app.config (cfg_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, cfg_group varchar(255) NOT NULL, cfg_key varchar(255) NOT NULL, cfg_val varchar(255), cfg_int bigint, cfg_num double precision, cfg_date timestamp(3), cfg_descr text, cfg_text text, CONSTRAINT g9_config_pkey PRIMARY KEY (cfg_id));
	CREATE TABLE app.log_error (log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_date timestamp(3) DEFAULT timezone('utc', now()), log_code bigint, log_msg varchar(255), log_data text, log_ip varchar(255), CONSTRAINT g9_log_error_pkey PRIMARY KEY (log_id));
	CREATE TABLE app.log_login ( log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_user uuid, log_ip varchar(255), log_ua text, log_data jsonb, log_date timestamp(3) DEFAULT timezone('utc'::text, now()), CONSTRAINT app_login_log_pkey PRIMARY KEY (log_id));
	CREATE TABLE app.session (sess_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, sess_key varchar(255), sess_data text, sess_expire timestamp(0), sess_extended integer, CONSTRAINT g9_session_pkey PRIMARY KEY (sess_id), CONSTRAINT g9_sess_key_unique UNIQUE (sess_key));
	CREATE TABLE app.api_keys (apk_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, apk_uid uuid not null, apk_key varchar(255) not null, apk_deklar integer not null, apk_exp date not null, apk_date timestamp(3) DEFAULT timezone('utc'::text, now()) not null, apk_user_id uuid, apk_del boolean default false, CONSTRAINT g9_apk_id_pkey PRIMARY KEY (apk_id), CONSTRAINT api_unique_key UNIQUE (apk_key));
END $$;


-- JAR Subscription (jei reikia)

-- Views setup:
DO LANGUAGE 'plpgsql' $$ BEGIN
	SET SESSION AUTHORIZATION "G9_admin"; 
	CREATE OR REPLACE VIEW g9.lkp_rodikliu_grupes AS SELECT lkp_num AS key, lkp_value AS val FROM g9.lookup WHERE lkp_group::text = 'RodikliuGrupe'::text;
	CREATE OR REPLACE VIEW g9.lkp_rodikliu_validacija AS SELECT lkp_num AS key, lkp_value AS val FROM g9.lookup WHERE lkp_group = 'RodikliuGrupe' and lkp_key='Taip';
	CREATE OR REPLACE VIEW g9.lkp_stebesenos AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='Stebesenos';
	CREATE OR REPLACE VIEW g9.lkp_statusas AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='Statusas';
	CREATE OR REPLACE VIEW g9.lkp_daznumas AS SELECT lkp_key as key, lkp_value as val FROM g9.lookup WHERE lkp_group='Daznumas';
	CREATE OR REPLACE VIEW g9.lkp_daznumo_laikas AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='DaznumoLaikas';
	CREATE OR REPLACE VIEW g9.lkp_daznumo_daugiklis AS SELECT lkp_num as key, lkp_int as val FROM g9.lookup WHERE lkp_group='DaznumoLaikas';
	CREATE OR REPLACE VIEW g9.lkp_vietos_tipas AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='VietosTipas';
	CREATE OR REPLACE VIEW g9.lkp_stebejimo_statusas AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='StebejimoStatusas';
	CREATE OR REPLACE VIEW g9.lkp_ruosimo_medziagos AS SELECT lkp_num as key, lkp_value as val FROM g9.lookup WHERE lkp_group='RuosimoMedziagos' ORDER BY lkp_sort;
	CREATE OR REPLACE VIEW g9.lkp_ruosimo_daznumas AS SELECT lkp_num as key, lkp_int as num, lkp_value as val FROM g9.lookup WHERE lkp_group='RuosimoDaznumas';
	CREATE OR REPLACE VIEW g9.v_deklar AS SELECT dkl_id AS "ID", dkl_gvts AS "GVTS", dkl_metai AS "Metai", dkl_stebesena As "Stebesenos", dkl_status AS "Statusas", dkl_kiekis AS "Kiekis", dkl_vartot AS "Vartotojai", dkl_medziagos as "RuosimoMedziagos", dkl_deklar_date AS "DeklarDate", dkl_deklar_user AS "DeklarUser", dkl_modif_date AS "RedagDate", dkl_modif_user AS "RedagUser", dkl_kontaktas_vardas as "KontaktaiVardas", dkl_kontaktas_pavarde as "KontaktaiPavarde", dkl_kontaktas_email as "KontaktaiEmail", dkl_kontaktas_phone as "KontaktaiPhone" FROM g9.deklaravimas;
	CREATE OR REPLACE VIEW g9.v_gvts AS SELECT vkl_id AS "ID", vkl_ja AS "JA", vkl_title AS "Title", vkl_adresas AS "Addr", vkl_gvtot as "GVTOT" FROM g9.gvts;
	CREATE OR REPLACE VIEW g9.v_gvts_ja AS SELECT g.vkl_id AS gvts, j.ja_kodas AS "ID", j.ja_pavadinimas AS "Title", j.adresas AS "Addr", jad_kontaktas_vardas AS "KontaktasVardas", jad_kontaktas_pavarde AS "KontaktasPavarde", jad_kontaktas_email AS "KontaktasEmail", jad_kontaktas_phone AS "KontaktasPhone" FROM g9.gvts g LEFT JOIN jar.data j ON g.vkl_ja = j.ja_kodas LEFT JOIN g9.ja_detales d ON g.vkl_ja = d.ja_id WHERE j.ja_kodas IS NOT NULL;
	CREATE OR REPLACE VIEW g9.v_ja_detales AS SELECT ja_kodas "ID", ja_pavadinimas "Title", adresas "Addr", d.jad_kontaktas_vardas "KontaktasVardas", d.jad_kontaktas_pavarde "KontaktasPavarde", d.jad_kontaktas_email "KontaktasEmail", d.jad_kontaktas_phone "KontaktasPhone" FROM jar.data as j LEFT JOIN g9.ja_detales d on j.ja_kodas=d.ja_id;
	CREATE OR REPLACE VIEW g9.v_rodikliai AS SELECT rod_id as "ID", rod_grupe as "Grupe", rod_kodas as "Kodas", rod_rodiklis as "Pavadinimas", rod_daznumas as "Daznumas", rod_min as "Min", rod_max as "Max", rod_step as "Step", rod_vnt as "Vnt", rod_aprasymas as "Aprasymas" FROM g9.rodikliai;
	CREATE OR REPLACE VIEW g9.v_reiksmes AS SELECT rks_id as "ID", rks_deklar as "Deklaracija", rks_rodiklis as "Rodiklis", rks_date as "Data", rks_reiksme as "Reiksme" FROM g9.reiksmes ORDER BY rks_rodiklis,rks_date;
	CREATE OR REPLACE VIEW g9.v_daznumas AS SELECT dzn_grupe as "Daznumas", dzn_nuo as "Nuo", dzn_iki as "Iki", dzn_kartai as "Kartai", dzn_laikas as "Laikas", l.val as "Daugiklis" FROM g9.daznumas as d left join g9.lkp_daznumo_daugiklis as l on (d.dzn_laikas=l.key) ORDER BY dzn_id ASC;
	CREATE OR REPLACE VIEW g9.v_gvts_users AS SELECT vkl_id "GVTS", u.user_id "ID", u.user_fname "FName", u.user_lname "LName", r.role_admin "Admin", u.user_dt_login "LastLogin" FROM g9.gvts as g LEFT JOIN app.roles as r on (g.vkl_id=r.role_gvts) LEFT JOIN app.users as u on (r.role_user=u.user_id) WHERE u.user_id is not null;
	CREATE OR REPLACE VIEW g9.lkp_virs_taisomas_veiksmas AS SELECT lkp_key AS key, lkp_value AS val FROM g9.lookup WHERE lkp_group::text = 'VirsTaisomasisVeiksmas'::text ORDER BY lkp_sort;
	CREATE OR REPLACE VIEW g9.lkp_virs_taisomas_priezastis AS SELECT lkp_num AS key, lkp_value AS val FROM g9.lookup WHERE lkp_group::text = 'VirsPriezastis'::text ORDER BY lkp_sort;
	CREATE OR REPLACE VIEW g9.v_rodikliai_suvedimas AS SELECT rks_id as "ID", rks_suvedimas as "Suvedimas", rod_kodas as "Kodas", rks_date as "Data", rks_reiksme as "Reiksme", rks_deklar as "Deklaracija" FROM g9.reiksmes LEFT JOIN g9.rodikliai ON (rks_rodiklis=rod_id);
	CREATE OR REPLACE VIEW g9.lkp_suvedimo_tipas AS SELECT lkp_num AS key, lkp_value AS val FROM g9.lookup WHERE lkp_group = 'SuvedimoTipas';
	CREATE OR REPLACE VIEW g9.v_deklar_keys AS SELECT apk_deklar "ID", apk_uid "RaktoID", apk_deklar AS "Deklaracija", dkl_gvts AS "GVTS", apk_exp "GaliojaIki", apk_date "Sukurtas", CONCAT(user_fname,' ',user_lname) "Autorius" FROM g9.deklaravimas LEFT JOIN app.api_keys on (dkl_id=apk_deklar) LEFT JOIN app.users on (apk_user_id=user_id) WHERE dkl_status in (1,2) and apk_id is not null and apk_del=false;
	CREATE OR REPLACE VIEW app.v_users AS SELECT user_id "ID", user_fname "FName", user_lname "LName", user_email "Email", user_phone "Phone" FROM app.users;
END $$;


--Funkcijos:
DO LANGUAGE 'plpgsql' $T$ BEGIN
	CREATE OR REPLACE FUNCTION g9.try_cast(_in text, INOUT _out ANYELEMENT) LANGUAGE plpgsql AS $$ BEGIN EXECUTE format('SELECT %L::%s', $1, pg_typeof(_out)) INTO _out; EXCEPTION WHEN others THEN END $$;	
	CREATE OR REPLACE FUNCTION g9.valid_kartojasi(deklar integer) RETURNS TABLE(rodiklis integer, data date, reiksme double precision) LANGUAGE 'plpgsql' AS $$ BEGIN RETURN QUERY SELECT rks_rodiklis, rks_date, rks_reiksme FROM g9.reiksmes WHERE rks_deklar=deklar GROUP BY rks_rodiklis, rks_date, rks_reiksme HAVING count(*)>1; END $$;
	CREATE OR REPLACE FUNCTION g9.valid_kartojasi_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Data" date, "Reiksme" double precision, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN RETURN QUERY SELECT vld_id, vld_rodiklis, vld_data, vld_reiksme, vld_tvirtinti, vld_pastabos FROM g9.valid_kartojasi WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE OR REPLACE FUNCTION g9.valid_kartojasi_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Data" date, "Reiksme" double precision, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN
		WITH curr AS (SELECT rodiklis, data, reiksme FROM g9.valid_kartojasi(deklar)),
		--Pažymėti neegzistuojančius
		upd as (UPDATE g9.valid_kartojasi SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND (vld_rodiklis,vld_data,vld_reiksme) NOT IN (SELECT rodiklis, data, reiksme FROM curr) RETURNING *),
		--Grąžinti ištrintus
		upf as (UPDATE g9.valid_kartojasi SET vld_del=false,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=true AND (vld_rodiklis,vld_data,vld_reiksme) IN (SELECT rodiklis, data, reiksme FROM curr) RETURNING *)
		--Pridėti naujus
		INSERT INTO g9.valid_kartojasi (vld_rodiklis,vld_data,vld_reiksme,vld_deklar,vld_user) SELECT rodiklis, data, reiksme, deklar, userid FROM curr WHERE (rodiklis, data, reiksme) NOT IN (SELECT vld_rodiklis,vld_data,vld_reiksme FROM g9.valid_kartojasi WHERE vld_deklar=deklar);
		RETURN QUERY SELECT * FROM g9.valid_kartojasi_get(deklar);
	END $$;

	CREATE OR REPLACE FUNCTION g9.valid_trukumas(deklar integer) RETURNS TABLE(rodiklis integer, suvesta integer, reikia integer) AS $$ DECLARE stb integer; kiekis integer; ruos integer[]; BEGIN
	SELECT COALESCE(dkl_kiekis,1),dkl_stebesena,dkl_medziagos into kiekis,stb,ruos FROM g9.deklaravimas WHERE dkl_id=deklar;
	RETURN QUERY WITH steb as (SELECT stb_rodiklis FROM g9.stebesenos WHERE stb_stebesenos=stb),
		dzn as (SELECT dzn_grupe,(max(dzn_kartai)*COALESCE(max(l.val),1)) as dzn_kartai FROM g9.daznumas d LEFT JOIN g9.lkp_daznumo_daugiklis l ON d.dzn_laikas = l.key WHERE dzn_stebesena=stb and dzn_nuo <=kiekis AND dzn_iki>=kiekis and dzn_kartai>0 group by dzn_grupe),
		abg as (SELECT rod_id,COALESCE(r.val,rod_daznumas) as rod_daznumas FROM g9.rodikliai LEFT JOIN g9.lkp_ruosimo_daznumas as r ON (rod_id=r.key and r.num = ANY(ruos))),
		rod as (SELECT rod_id,dzn_kartai FROM abg INNER JOIN dzn ON (rod_daznumas=dzn_grupe)),
		rks as (SELECT rks_rodiklis, count(*) as rks_count FROM g9.reiksmes WHERE rks_deklar=deklar group by rks_rodiklis),
		grp as (SELECT rod_id,dzn_kartai FROM steb INNER JOIN rod on (rod_id=stb_rodiklis))
	SELECT rod_id,COALESCE(rks_count,0)::integer, dzn_kartai FROM grp LEFT JOIN rks ON (rks_rodiklis=rod_id) WHERE dzn_kartai>rks_count or rks_count is null;
	END $$ LANGUAGE 'plpgsql';

	CREATE OR REPLACE FUNCTION g9.valid_suvesti(deklar integer) RETURNS TABLE("Rodiklis" integer, "Reikia" integer) AS $$ DECLARE stb integer; kiekis integer; ruos integer[]; BEGIN
	SELECT COALESCE(dkl_kiekis,1),dkl_stebesena,dkl_medziagos into kiekis,stb,ruos FROM g9.deklaravimas WHERE dkl_id=deklar;
	IF(kiekis<10) THEN kiekis=10; END IF;
	RETURN QUERY WITH steb as (SELECT stb_rodiklis FROM g9.stebesenos WHERE stb_stebesenos=stb),
		dzn as (SELECT dzn_grupe,(max(dzn_kartai)*COALESCE(max(l.val),1)) as dzn_kartai FROM daznumas d LEFT JOIN g9.lkp_daznumo_daugiklis l ON d.dzn_laikas = l.key WHERE dzn_stebesena=stb and dzn_nuo <=kiekis AND dzn_iki>=kiekis and dzn_kartai>0 group by dzn_grupe),
		abg as (SELECT rod_id,COALESCE(r.val,rod_daznumas) as rod_daznumas FROM rodikliai LEFT JOIN g9.lkp_ruosimo_daznumas as r ON (rod_id=r.key and r.num = ANY(ruos))),
		rod as (SELECT rod_id,dzn_kartai FROM abg INNER JOIN dzn ON (rod_daznumas=dzn_grupe)),
		grp as (SELECT rod_id,dzn_kartai FROM steb INNER JOIN rod on (rod_id=stb_rodiklis))
	SELECT rod_id, dzn_kartai FROM grp;
	END $$ LANGUAGE 'plpgsql';

	CREATE OR REPLACE FUNCTION g9.valid_trukumas_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Suvesta" integer, "Reikia" integer, "KitasDaznumas" boolean, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$BEGIN RETURN QUERY
	SELECT vld_id, vld_rodiklis, vld_suvesta, vld_reikia, vld_kitas, vld_tvirtinti, vld_pastabos FROM g9.valid_trukumas WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE OR REPLACE FUNCTION g9.valid_trukumas_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Suvesta" integer, "Reikia" integer, "KitasDaznumas" boolean, "Patvirtinta" boolean, "Pastabos" text) LANGUAGE 'plpgsql' AS $$ BEGIN
	   WITH curr AS (SELECT rodiklis, suvesta, reikia FROM g9.valid_trukumas(deklar)),
	   --Pažymėti neegzistuojančius
	   upd as (UPDATE g9.valid_trukumas SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND vld_rodiklis NOT IN (SELECT rodiklis FROM curr) RETURNING *),
	   --Grąžinti ištrintus
	   upf as (UPDATE g9.valid_trukumas SET vld_del=false,vld_suvesta=curr.suvesta,vld_reikia=curr.reikia,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) FROM curr WHERE vld_deklar=deklar AND vld_rodiklis=curr.rodiklis AND (vld_del=true or vld_rodiklis<>curr.rodiklis or vld_suvesta<>curr.suvesta) RETURNING *)
	   --Pridėti naujus
	   INSERT INTO g9.valid_trukumas (vld_rodiklis,vld_suvesta,vld_reikia,vld_deklar,vld_user) SELECT rodiklis, suvesta, reikia, deklar, userid FROM curr WHERE rodiklis NOT IN (SELECT vld_rodiklis FROM g9.valid_trukumas WHERE vld_deklar=deklar);   
	   RETURN QUERY SELECT * FROM g9.valid_trukumas_get(deklar);
	END $$; 
	
	CREATE OR REPLACE FUNCTION g9.valid_virsija(in deklar integer, out rodiklis integer, out nuo date, out iki date, out max double precision) RETURNS SETOF RECORD AS $$ DECLARE rec RECORD; BEGIN max:=0;
	   FOR rec IN (WITH 
	      riba as (SELECT rks_rodiklis rod, rks_date dte FROM reiksmes LEFT JOIN g9.rodikliai on (rks_rodiklis=rod_id) WHERE rks_deklar=deklar and (rks_reiksme>rod_max or rks_reiksme<rod_min) GROUP BY rks_rodiklis,rks_date),
	      rod as (SELECT rks_rodiklis, rks_date, rks_reiksme, rod is not null vl_virs FROM reiksmes LEFT JOIN riba on (rks_rodiklis=rod and rks_date=dte) WHERE rks_deklar=deklar)
	      SELECT rks_rodiklis as rod, rks_date as dte, rks_reiksme as val, vl_virs as virs, LEAD(vl_virs) OVER (PARTITION BY rks_rodiklis ORDER BY rks_date) IS DISTINCT FROM vl_virs as ends FROM rod
	   ) LOOP IF rec.virs THEN rodiklis:=rec.rod; iki:=rec.dte; IF nuo IS NULL THEN nuo:=rec.dte; END IF; IF max<rec.val THEN max:=rec.val; END IF; IF rec.ends THEN RETURN NEXT; nuo:=null; END IF; END IF;
	END LOOP; RETURN; END $$ LANGUAGE plpgsql;
	CREATE OR REPLACE FUNCTION g9.valid_virsija_get(deklar integer) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Nuo" date, "Iki" date, "Max" double precision, "Detales" boolean, "Nereiksmingas" boolean, "NereiksmApras" text, "Zmones" integer, "Tipas" integer, "LOQVerte" boolean, "LOQReiksme" double precision, "Statusas" integer, "Patvirtinta" boolean, "Pastabos" text, "Priezastis" integer, "Veiksmas" varchar, "Pradzia" date, "Pabaiga" date) LANGUAGE 'plpgsql' AS $$BEGIN RETURN QUERY
	WITH rod as (SELECT rod_id as id FROM g9.lkp_rodikliu_validacija as vld LEFT JOIN g9.rodikliai on (rod_grupe=vld.key)) SELECT vld_id, vld_rodiklis, vld_nuo, vld_iki, vld_max, rod.id is not null, vld_nereiksm, vld_nereiksm_apras, vld_zmones, vld_tipas, vld_loq_verte, vld_loq_reiksme, vld_statusas, vld_tvirtinti, vld_pastabos, vld_priez, vld_veiksmas, vld_pradzia, vld_pabaiga FROM g9.valid_virsija LEFT JOIN rod ON (vld_rodiklis=rod.id) WHERE vld_deklar=deklar and vld_del=false; END; $$;
	CREATE OR REPLACE FUNCTION g9.valid_virsija_set(deklar integer, userid uuid) RETURNS TABLE("ID" integer, "Rodiklis" integer, "Nuo" date, "Iki" date, "Max" double precision, "Detales" boolean, "Nereiksmingas" boolean, "NereiksmApras" text, "Zmones" integer, "Tipas" integer, "LOQVerte" boolean, "LOQReiksme" double precision, "Statusas" integer, "Patvirtinta" boolean, "Pastabos" text, "Priezastis" integer, "Veiksmas" varchar, "Pradzia" date, "Pabaiga" date) LANGUAGE 'plpgsql' AS $$ BEGIN
	   WITH curr AS (SELECT rodiklis, nuo, iki, max FROM g9.valid_virsija(deklar)),
	   --Pažymėti neegzistuojančius
	   upd as (UPDATE g9.valid_virsija SET vld_del=true,vld_user=userid,vld_date_modif=timezone('utc'::text, now()) WHERE vld_deklar=deklar AND vld_del=false AND (vld_rodiklis,vld_nuo,vld_iki) NOT IN (SELECT rodiklis,nuo,iki FROM curr) RETURNING *),
	   --Grąžinti ištrintus
	   upf as (UPDATE g9.valid_virsija SET vld_del=false,vld_max=curr.max, vld_user=userid,vld_date_modif=timezone('utc'::text, now()) FROM curr WHERE vld_deklar=deklar AND vld_rodiklis=curr.rodiklis AND vld_nuo=curr.nuo AND vld_iki=curr.iki AND (vld_del=true or vld_max<>curr.max) RETURNING *)
	   --Pridėti naujus
	   INSERT INTO g9.valid_virsija (vld_rodiklis,vld_nuo,vld_iki,vld_max,vld_deklar,vld_user)  SELECT rodiklis, nuo, iki, max, deklar, userid FROM curr WHERE (rodiklis,nuo,iki) NOT IN (SELECT vld_rodiklis,vld_nuo,vld_iki FROM g9.valid_virsija WHERE vld_deklar=deklar);   
	   RETURN QUERY SELECT * FROM g9.valid_virsija_get(deklar);
	END $$; 

	CREATE OR REPLACE FUNCTION g9.valid_nepatvirtinta(deklar integer) RETURNS TABLE(kartojasi integer, trukumas integer, virsija integer) AS $$ BEGIN RETURN QUERY SELECT (SELECT count(*)::int FROM g9.valid_kartojasi WHERE vld_deklar=deklar and vld_del<>true and vld_tvirtinti<>true) kartojasi, (SELECT count(*)::int FROM g9.valid_trukumas WHERE vld_deklar=deklar and vld_del<>true and vld_tvirtinti<>true) trukumas, (SELECT count(*)::int FROM g9.valid_virsija where vld_deklar=deklar and vld_del<>true and vld_tvirtinti<>true) virsija; END; $$ LANGUAGE plpgsql;
	CREATE OR REPLACE FUNCTION g9.valid_virsija_detales(vld integer) RETURNS bool AS $$ DECLARE ret bool; BEGIN SELECT rvl.key is not null FROM g9.valid_virsija LEFT JOIN g9.rodikliai ON vld_rodiklis=rod_id LEFT JOIN g9.lkp_rodikliu_validacija as rvl ON (rod_grupe=rvl.key) WHERE vld_id=vld INTO ret; RETURN ret; END $$ LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION app.api_keys(deklar integer) RETURNS table("Key" varchar(255), "Exp" date, "Date" timestamp(0), "User" varchar(255)) AS $$ BEGIN RETURN QUERY SELECT apk_key, apk_exp, apk_date, CONCAT(user_fname,' ',user_lname)::varchar(255) FROM app.api_keys LEFT JOIN app.users on (apk_user=user_id) WHERE apk_deklar=1; END $$ LANGUAGE plpgsql;
	CREATE OR REPLACE FUNCTION app.api_auth(apikey varchar(255)) RETURNS table("ID" uuid, "Key" varchar(255), "Deklar" integer, "Exp" date, "Metai" integer) AS $$ BEGIN RETURN QUERY SELECT apk_uid, apk_key, apk_deklar, apk_exp, dkl_metai FROM app.api_keys LEFT JOIN g9.deklaravimas ON (apk_deklar=dkl_id) WHERE apk_key=apikey AND apk_exp >= now()::date AND dkl_id is not null AND dkl_status IN (1,2); END $$ LANGUAGE plpgsql;
	CREATE OR REPLACE FUNCTION app.api_key_add(apikey varchar(255), gvts bigint, dkl integer, expire date, usr uuid) RETURNS TABLE(id uuid, key varchar, deklar int, exp date) AS $$ DECLARE uid uuid = gen_random_uuid(); BEGIN IF NOT EXISTS(SELECT 1 FROM app.api_keys WHERE apk_key=apikey) AND EXISTS(SELECT 1 FROM g9.deklaravimas WHERE dkl_id=dkl and dkl_gvts=gvts and dkl_status in (1,2)) THEN INSERT INTO app.api_keys (apk_uid,apk_key,apk_deklar,apk_exp,apk_user_id,apk_del) VALUES (uid,apikey,dkl,expire,usr,false); RETURN QUERY SELECT apk_uid, apk_key, apk_deklar, apk_exp FROM app.api_keys WHERE apk_uid=uid; END IF; END; $$ LANGUAGE plpgsql;
	CREATE OR REPLACE FUNCTION app.api_key_del(api uuid, gvts bigint, deklar integer) RETURNS boolean AS $$ BEGIN IF EXISTS(SELECT * FROM g9.deklaravimas WHERE dkl_id=deklar and dkl_gvts=gvts and dkl_status in (1,2)) THEN UPDATE app.api_keys SET apk_del=true WHERE apk_uid=api and akp_deklar=deklar; RETURN true; ELSE RETURN false; END IF; END; $$ LANGUAGE plpgsql;

END $T$;




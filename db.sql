EXEC UserSetup;
EXEC JarSetup;

GRANT USAGE ON SCHEMA public TO g9_app; GRANT USAGE ON SCHEMA jar TO g9_app;

--Public Tables
CREATE TABLE IF NOT EXISTS public.daznumas (dzn_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dzn_grupe varchar(3) NOT NULL, dzn_pavad varchar(255) NOT NULL, dzn_nuo integer NOT NULL, dzn_iki integer NOT NULL, dzn_kartai integer NOT NULL, dzn_laikas integer NOT NULL, CONSTRAINT g9_daznumas_pkey PRIMARY KEY (dzn_id));
CREATE TABLE IF NOT EXISTS public.deklaravimas (dkl_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, dkl_gvts bigint NOT NULL, dkl_metai integer NOT NULL, dkl_status varchar(30) NOT NULL, dkl_kiekis double precision, dkl_vartot integer, dkl_deklar_date timestamp(3), dkl_deklar_user varchar(255), dkl_deklar_user_id uuid, dkl_modif_date timestamp(3), dkl_modif_user varchar(255), dkl_modif_user_id uuid, CONSTRAINT g9_deklaravimas_pkey PRIMARY KEY (dkl_id));
CREATE TABLE IF NOT EXISTS public.gvts (vkl_id bigint NOT NULL, vkl_ja bigint, vkl_title varchar(255), vkl_saviv varchar(255), vkl_adresas varchar(255), CONSTRAINT g9_gvts_pkey PRIMARY KEY (vkl_id));
CREATE TABLE IF NOT EXISTS public.lookup (lkp_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, lkp_group varchar(30) NOT NULL, lkp_key varchar(30), lkp_num integer, lkp_value varchar(255), CONSTRAINT g9_public_lookup_pkey PRIMARY KEY (lkp_id));
CREATE TABLE IF NOT EXISTS public.rodikliai (rod_id integer NOT NULL, rod_stebesena integer NOT NULL, rod_kodas varchar(30) NOT NULL, rod_rodiklis varchar(255) NOT NULL, rod_daznumas varchar(3), rod_min double precision NOT NULL, rod_max double precision NOT NULL, rod_step double precision NOT NULL, rod_vnt varchar(30) NOT NULL, rod_aprasymas text, CONSTRAINT g9_rodikliai_pkey PRIMARY KEY (rod_id));
GRANT SELECT ON TABLE public.daznumas TO g9_app; GRANT ALL ON TABLE public.deklaravimas TO g9_app; GRANT SELECT ON TABLE public.gvts TO g9_app; GRANT SELECT ON TABLE public.lookup TO g9_app; GRANT SELECT ON TABLE public.rodikliai TO g9_app;

--Public Views
CREATE OR REPLACE VIEW public.v_deklar AS SELECT dkl_gvts AS "GVTS", dkl_metai AS "Metai", dkl_status AS "Statusas", dkl_kiekis AS "Kiekis", dkl_vartot AS "Vartotojai", dkl_deklar_date AS "DeklarDate", dkl_deklar_user AS "DeklarUser", dkl_modif_date AS "RedagDate", dkl_modif_user AS "RedagUser" FROM deklaravimas;
CREATE OR REPLACE VIEW public.v_gvts AS SELECT vkl_id AS "ID", vkl_ja AS "JA", vkl_title AS "Title", vkl_adresas AS "Addr" FROM gvts;
CREATE OR REPLACE VIEW public.v_gvts_ja AS SELECT g.vkl_id AS gvts, j.ja_id AS "ID", j.ja_title AS "Title", j.ja_adresas AS "Addr" FROM gvts g LEFT JOIN jar.data j ON g.vkl_ja = j.ja_id WHERE j.ja_id IS NOT NULL;
CREATE OR REPLACE VIEW public.v_stebesenos AS SELECT lkp_num AS key, lkp_value AS val FROM lookup WHERE lkp_group='Stebesena';
GRANT SELECT ON TABLE public.v_gvts TO g9_app; GRANT SELECT ON TABLE public.v_deklar TO g9_app; GRANT SELECT ON TABLE public.v_gvts_ja TO g9_app; GRANT SELECT ON TABLE public.v_stebesenos TO g9_app;

--App Tables
CREATE TABLE IF NOT EXISTS app.config (cfg_id integer NOT NULL GENERATED ALWAYS AS IDENTITY, cfg_group varchar(255) NOT NULL, cfg_key varchar(255) NOT NULL, cfg_val varchar(255), cfg_int bigint, cfg_num double precision, cfg_date timestamp(3), cfg_descr text, cfg_text text, CONSTRAINT g9_config_pkey PRIMARY KEY (cfg_id));
CREATE TABLE IF NOT EXISTS app.log_error (log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_date timestamp(3) DEFAULT timezone('utc', now()), log_code bigint, log_msg varchar(255), log_data text, log_ip varchar(255), CONSTRAINT g9_log_error_pkey PRIMARY KEY (log_id));
CREATE TABLE IF NOT EXISTS app.log_login (log_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, log_data text, log_date timestamp(3) DEFAULT timezone('utc', now()), CONSTRAINT g9_login_log_pkey PRIMARY KEY (log_id));
CREATE TABLE IF NOT EXISTS app.session (sess_id bigint NOT NULL GENERATED ALWAYS AS IDENTITY, sess_key varchar(255), sess_data text, sess_expire timestamp(0), sess_extended integer, CONSTRAINT g9_session_pkey PRIMARY KEY (sess_id), CONSTRAINT g9_sess_key_unique UNIQUE (sess_key));
GRANT SELECT, UPDATE ON TABLE app.config TO g9_app; GRANT INSERT ON TABLE app.log_error TO g9_app; GRANT INSERT ON TABLE app.log_login TO g9_app; GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE app.session TO g9_app;




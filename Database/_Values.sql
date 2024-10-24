
INSERT INTO app.config (cfg_group,cfg_key,cfg_val,cfg_int,cfg_num,cfg_date,cfg_descr,cfg_text) VALUES
	('Config','DebugDB',null,1,null,null,'Spausdinti kodo vykdymą konsolėje',null),
	('Auth','Host', 'http://localhost:5505/auth/v2/TestSecret2',null,null,null,'Autorizacijos portalo adresas',null),
	('Auth','Token',null,null,null,null,null,'slaptas kodas'),
	('Auth','Return','/api/user',null,null,null,null,'slaptas kodas'),
	('Session','Extend',null,3600,null,null,'Sesijos pratesimo laikas sekundėmis',null),
	('Session','Expire',null,86400,null,null,'Sesijos galiojimo laikas sekundėmis',null),
	('Session','KeyLength',null,64,null,null,'Sesijos rakto ilgis',null),
	('Web','Path', '/',null,null,null,'Sistemos pagrindinė direktorija',null);


INSERT INTO g9.daznumas (dzn_grupe,dzn_nuo,dzn_iki,dzn_kartai,dzn_laikas,dzn_stebesena) VALUES
	('A',0,9,1,1,1),('A',10,100,2,1,1),('A',101,1000,4,1,1),('A',1001,2000,7,1,1),('A',2001,3000,10,1,1),('A',3001,4000,13,1,1),('A',4001,5000,16,1,1),('A',5001,6000,19,1,1),('A',6001,7000,22,1,1),
	('A',7001,8000,25,1,1),('A',8001,9000,28,1,1),('A',9001,10000,31,1,1),('A',10001,11000,34,1,1),('A',11001,12000,37,1,1),('A',12001,11300,40,1,1),('A',13001,14000,43,1,1),('A',14001,15000,46,1,1),
	('A',15001,16000,49,1,1),('A',16001,17000,52,1,1),('A',17001,18000,55,1,1),('A',18001,19000,58,1,1),('A',19001,20000,61,1,1),('A',20001,21000,64,1,1),('A',21001,22000,67,1,1),('A',22001,23000,70,1,1),
	('A',23001,24000,73,1,1),('A',24001,25000,76,1,1),('A',25001,26000,79,1,1),('A',26001,27000,82,1,1),('A',27001,28000,85,1,1),('A',28001,29000,88,1,1),('A',29001,30000,91,1,1),('A',30001,31000,94,1,1),
	('A',31001,32000,97,1,1),('A',32001,33000,100,1,1),('A',33001,34000,103,1,1),('A',34001,35000,106,1,1),('A',35001,36000,109,1,1),('A',36001,37000,112,1,1),('A',37001,38000,115,1,1),('A',38001,39000,118,1,1),
	('A',39001,40000,121,1,1),('A',40001,41000,124,1,1),('A',41001,42000,127,1,1),('A',42001,43000,130,1,1),('A',43001,44000,133,1,1),('A',44001,45000,135,1,1),('A',45001,46000,138,1,1),('A',46001,47000,141,1,1),
	('A',47001,48000,144,1,1),('A',48001,49000,147,1,1),('A',49001,50000,150,1,1),('A',50001,60000,153,1,1),('B',0,9,0,5,1),('B',10,1000,1,1,1),('B',1001,5500,2,1,1),('B',5501,10000,3,1,1),('B',10001,20000,4,1,1),
	('B',20001,30000,5,1,1),('B',30001,40000,6,1,1),('B',40001,50000,7,1,1),('B',50001,60000,8,1,1),('Dr1',0,1000,1,1,2),('Dr2',1001,10000,1,1,2),('Dr3',10001,60000,1,1,2),('Rad',0,100,0,6,1),('Rad',101,1000,1,1,1),
	('Rad',1001,4300,2,1,1),('Rad',4301,7600,3,1,1),('Rad',7601,10000,4,1,1),('Rad',10001,20000,4,1,1),('Rad',20001,30000,5,1,1),('Rad',30001,40000,6,1,1),('Rad',40001,50000,7,1,1),('Rad',50001,60000,8,1,1);

INSERT INTO g9.lookup(lkp_group,lkp_key,lkp_num,lkp_value,lkp_int,lkp_sort) VALUES
	('Daznumas','A',null,'A Grupė',null,null),('Daznumas','B',null,'B Grupė',null,null),('Daznumas','Rad',null,'Radiologiniai rodikliai',null,null),
	('RodikliuGrupe','Taip',1,'Mikrobiologiniai rodikliai',null,null),('RodikliuGrupe','Taip',2,'Cheminiai rodikliai',null,null),('VietosTipas',null,1,'Reprezentatyvi vieta pastatų vidaus vandentiekio tinkle',null,null),
	('VietosTipas',null,2,'Čiaupas patalpoje ar objekte',null,null),('VietosTipas',null,3,'Vieta, kurioje vanduo  išpilstomas į butelius ar talpyklas',null,null),('VietosTipas',null,4,'Vieta, kurioje vanduo naudojamas maisto tvarkymo įmonėje',null,null),
	('VietosTipas',null,5,'Vieta, kurioje vanduo išteka iš cisternos',null,null),('StebejimoStatusas',null,1,'Įrašas patvirtinamas kaip teisingas',null,null),('StebejimoStatusas',null,2,'Trūksta stebimos vertės, stebima vertė nėra svarbi arba nereikšminga',null,null),
	('StebejimoStatusas',null,3,'Trūksta pastebėtos vertės, daugiau informacijos nėra',null,null),('Daznumas','Dr1',null,'Drumstumas vandens tiekimo įmonėje',null,null),('Daznumas','Dr2',null,'Drumstumas vandens tiekimo įmonėje',null,null),
	('Daznumas','Dr3',null,'Drumstumas vandens tiekimo įmonėje',null,null),('RuosimoMedziagos',null,1,'Chloraminas',null,null),('RuosimoMedziagos',null,2,'Flokuliantas, kuriame yra aliuminio',null,null),
	('RuosimoMedziagos',null,3,'Flokuliantas, kuriame yra geležies',null,null),('RodikliuGrupe',null,6,'Veiklos stebėsenos rodikliai',null,null),('DaznumoLaikas',null,1,'Metai',1,null),('DaznumoLaikas',null,2,'Mėnuo',12,null),
	('DaznumoLaikas',null,3,'Savaitė',52,null),('DaznumoLaikas',null,4,'Diena',365,null),('DaznumoLaikas',null,5,'Per 6 metus',0,null),('DaznumoLaikas',null,6,'Per 10 metų',0,null),('DaznumoLaikas',null,7,'Nuolat',365,null),
	('RuosimoDaznumas',null,27,'A',1,null),('RuosimoDaznumas',null,43,'A',1,null),('RuosimoDaznumas',null,44,'A',2,null),('RuosimoDaznumas',null,50,'A',3,null),('Statusas',null,1,'Pildoma',null,null),
	('Statusas',null,2,'Deklaruojama',null,null),('Statusas',null,3,'Deklaruota',null,null),('Stebesenos',null,1,'Geriamo vandens stebėsenos',null,null),('RuosimoMedziagos',null,4,'Kitos',null,null),
	('RodikliuGrupe',null,4,'Radiologiniai rodikliai',null,null),('RodikliuGrupe',null,5,'Dezinfekuoti naudojamų medžiagų likučiai',null,null),('RodikliuGrupe',null,3,'Indikatoriniai rodikliai',null,null),
	('Stebesenos',null,2,'Veiklos stebėsenos',null,null),('VirsPriezastis',null,1,'Atsitiktinė tarša',null,null),('VirsPriezastis',null,2,'Potvynis',null,null),('VirsPriezastis',null,3,'Protrūkis',null,null),
	('VirsPriezastis',null,4,'Fizinė nelaimė',null,null),('VirsPriezastis',null,5,'Užsitęsusi sausra',null,null),('VirsPriezastis',null,6,'Ruošimo klaida',null,null),('VirsPriezastis',null,7,'Neplanuotas geriamojo vandens tiekimo pertrūkis',null,null),
	('VirsPriezastis',null,8,'Vandens trūkumas',null,null),('VirsPriezastis',null,9,'Kita',null,null),('VirsPriezastis',null,10,'Nežinoma',null,null),('VirsTaisomasisVeiksmas','C1',null,'Susijęs su vandens baseinu: veiksmai, skirti priežasčiai pašalinti arba sušvelninti (C1)',null,1),
	('VirsTaisomasisVeiksmas','C2',null,'Susijęs su vandens baseinu: šaltinio pakeitimo veiksmai (C2)',null,2),('VirsTaisomasisVeiksmas','D1',null,'Susijęs su pastatų vidaus vandentiekiu: sugedusių komponentų keitimas, atjungimas arba taisymas (D1)',null,3),
	('VirsTaisomasisVeiksmas','D2',null,'Susijęs su pastatų vidaus vandentiekiu: užterštų komponentų valymas, šveitimas ir (arba) dezinfekavimas (D2)',null,4),('VirsTaisomasisVeiksmas','E1',null,'Skubūs veiksmai vartotojų sveikatai ir saugai: vartotojų informavimas ir nurodymai, pavyzdžiui, draudimas naudoti, vandenį virinti, užvirinimo užsakymas, laikinai riboti vartojimą (E1)',null,5),
	('VirsTaisomasisVeiksmas','E2',null,'Skubūs veiksmai vartotojų sveikatai ir saugai: laikinas alternatyvus geriamojo vandens tiekimas, pvz., vanduo buteliuose, vanduo taroje, cisternos (E2)',null,6),
	('VirsTaisomasisVeiksmas','E3',null,'Skubūs veiksmai vartotojų sveikatai ir saugai: geriamojo vandens vartojimo apribojimas jautriems naudotojams (E3)',null,7),('VirsTaisomasisVeiksmas','E4',null,'Skubūs veiksmai vartotojų sveikatai ir saugai: geriamojo vandens tiekimo uždraudimas (E4)',null,8),
	('VirsTaisomasisVeiksmas','P1',null,'Susijęs su vandens tiekimo skirstomuoju tinklu: sugedusių komponentų keitimas, atjungimas arba taisymas (P1)',null,9),('VirsTaisomasisVeiksmas','P2',null,'Susijęs su vandens tiekimo skirstomuoju tinklu: užterštų komponentų valymas, šveitimas ir (arba) dezinfekavimas (P2)',null,10),
	('VirsTaisomasisVeiksmas','S1',null,'Apsaugos priemonės, apsaugančios nuo neteisėtos prieigos (S1)',null,11),('VirsTaisomasisVeiksmas','T1',null,'Susijęs su paruošimu: paruošimo įdiegimas, atnaujinimas arba tobulinimas (T1)',null,12),
	('VirsTaisomasisVeiksmas','N',null,'Nėra (N)',null,13),('VirsTaisomasisVeiksmas','O',null,'Kita (O)',null,14),('SuvedimoTipas',null,1,'Vartotojas',null,null),('SuvedimoTipas',null,2,'Excel',null,null),('SuvedimoTipas',null,3,'API',null,null);



INSERT INTO g9.rodikliai (rod_id,rod_grupe,rod_kodas,rod_rodiklis,rod_daznumas,rod_min,rod_max,rod_step,rod_vnt,rod_aprasymas) VALUES
	(1,1,'EEA_15-01-0','Žarninės lazdelės (Escherichia coli) ','A',0,0,1,'skaičius/100ml',''),(31,2,'CAS_60-57-1','Dieldrinas','B',0,0.03,0.001,'µg/l ','̶'),(32,2,'CAS_76-44-8','Heptachloras','B',0,0.03,0.001,'µg/l','̶'),
	(33,2,'CAS_1024-57-3','Heptachlorepoksidas','B',0,0.03,0.001,'µg/l','̶'),(34,2,'VMVT_002','Pesticidų suma ','B',0,0.5,0.01,'µg/l','Pesticidų suma – visų atskirų pesticidų, nurodytų pesticidų grupėje, nustatytų atliekant geriamojo vandens stebėseną, verčių suma.'),
	(35,2,'VMVT_003','PFAS iš viso','B',0,0.5,0.01,'µg/l','Rodiklio ribinė vertė taikoma perfluoralkilintų ir polifluoralkilintų cheminių medžiagų verčių sumai. 
	Iki 2026 m. sausio 12 d. nereikalaujama vykdyti šio rodiklio stebėsenos. Nuo tos datos šio rodiklio stebėsena turi būti vykdoma tuo atveju, jei yra atliktas vandens gavybos vietoms skirtų vandens baseinų rizikos vertinimas ir jo rezultatai rodo, kad yra tikėtinas tų medžiagų buvimas konkrečioje geriamojo vandens tiekimo sistemoje.'),
	(36,2,'EEA_33-74-9','PFAS suma ','B',0,0.1,0.001,'µg/l','Rodiklio ribinė vertė taikoma perfluoralkilintų ir polifluoralkilintų cheminių medžiagų, kurios galėtų kelti pavojų geriamojo vandens saugai ir kokybei, verčių sumai: perfluorbutano rūgšties (PFBA), perfluorpentano rūgšties (PFPA), perfluorheksano rūgšties (PFHxA), perfluorheptano rūgšties (PFHpA), perfluoroktano rūgšties (PFOA), perfluornonano rūgšties (PFNA), perfluordekano rūgšties (PFDA), perfluoroundekano rūgšties (PFUnDA), perfluorododekano rūgšties (PFDoDA), perfluorotridekano rūgšties (PFTrDA), perfluorbutansulfono rūgšties (PFBS), perfluorpentansulfono rūgšties (PFPS), perfluor-heksansulfono rūgšties (PFHxS), perfluorheptansulfono rūgšties (PFHpS), perfluoroktansulfono rūgšties (PFOS), perfluornonansulfono rūgšties (PFNS), perfluor-dekansulfono rūgšties (PFDS), perfluorundekansulfono rūgšties, perfluordodekansulfono rūgšties, perfluortri-dekansulfono rūgšties. 
	„PFAS suma“ yra „PFAS iš viso“ cheminių medžiagų pogrupis, kurio sudėtyje yra perfluoralkilinta trijų arba daugiau kaip trijų anglies atomų dalis (t. y. – CnF2n–, n ≥ 3) arba perfluoroalkileterio dviejų arba daugiau kaip dviejų anglies atomų dalis (t. y. –CnF2nOCmF2m–, n ir m ≥ 1).
	Iki 2026 m. sausio 12 d. nereikalaujama vykdyti šio rodiklio stebėsenos. Nuo tos datos šio rodiklio stebėsena turi būti vykdoma tuo atveju, jei yra atliktas vandens gavybos vietoms skirtų vandens baseinų rizikos vertinimas ir jo rezultatai rodo, kad yra tikėtinas tų medžiagų buvimas konkrečioje geriamojo vandens tiekimo sistemoje.'),
	(2,1,'EEA_15-02-1','Žarniniai enterokokai','A',0,0,1,'skaičius/100ml',''),(4,2,'CAS_7440-36-0','Stibis ','B',0,10,0.1,'µg/l ',''),(5,2,'CAS_7440-38-2','Arsenas ','B',0,10,0.1,'µg/l ',''),
	(3,2,'CAS_79-06-1','Akrilamidas','B',0,0.1,0.001,'µg/l','Rodiklio ribinė vertė reiškia monomero likučių koncentraciją geriamajame vandenyje, apskaičiuotą pagal geriamojo vandens ruošimo ir tiekimo priemonių specifikacijas, kuriose nurodyta, kiek daugiausia jo išsiskiria iš atitinkamo polimero, besiliečiančio su geriamuoju vandeniu.'),
	(8,2,'CAS_80-05-7','Bisfenolis A','B',0,2.5,0.1,'µg/l','Rodiklio ribinė vertė taikoma nuo 2026 m. sausio 12 d. Iki tos datos nereikalaujama vykdyti šio rodiklio stebėsenos.'),
	(9,2,'CAS_7440-42-8','Boras','B',0,1.5,0.001,'mg/l','Taikoma 2,4 mg/l rodiklio ribinė vertė, kai pagrindinis geriamojo vandens šaltinis yra gėlintas vanduo, arba vandenvietėse, kuriose dėl geologinių sąlygų gali susidaryti didelės boro koncentracijos požeminiame vandenyje.'),
	(10,2,'CAS_15541-45-4','Bromatas','B',0,10,0.1,'µg/l','̶'),(6,2,'CAS_71-43-2','Benzenas','B',0,1,0.01,'µg/l',''),(7,2,'CAS_50-32-8','Benzo(a)pirenas','B',0,0.01,0.0001,'µg/l ',''),
	(12,2,'CAS_14866-68-3','Chloratas','B',0,0.25,0.001,'mg/l','Taikoma 0,70 mg/l rodiklio ribinė vertė, kai geriamajam vandeniui dezinfekuoti naudojamas metodas, kurio metu susidaro chloratas, visų pirma kai dezinfekuoti naudojamas chloro dioksidas. Šis rodiklis matuojamas tik tuo atveju, jei naudojami tokie dezinfekavimo metodai.'),
	(13,2,'CAS_14998-27-7','Chloritas','B',0,0.25,0.001,'mg/l','Taikoma 0,70 mg/l rodiklio ribinė vertė, kai geriamajam vandeniui dezinfekuoti naudojamas dezinfekavimo metodas, kurio metu susidaro chloritas, visų pirma, kai dezinfekuoti naudojamas chloro dioksidas. Šis rodiklis matuojamas tik tuo atveju, jei naudojami tokie dezinfekavimo metodai.'),
	(14,2,'CAS_7440-47-3','Chromas','B',0,50,0.1,'µg/l','Iki 2036 m. sausio 12 d. taikoma rodiklio ribinė vertė – 50 µg/l,  po šios datos - 25 µg/l.'),
	(15,2,'CAS_7440-50-8','Varis ','B',0,2,0.01,'mg/l ','̶'),(11,2,'CAS_7440-43-9','Kadmis ','B',0,5,0.1,'µg/l','̶'),(16,2,'CAS_57-12-5','Cianidas ','B',0,50,1,'µg/l','Rodiklio ribinė vertė taikoma visų formų cianidų verčių sumai.'),
	(17,2,'CAS_107-06-2','1,2-dichloretanas ','B',0,3,0.01,'µg/l','̶'),
	(18,2,'CAS_106-89-8','Epichlorhidrinas','B',0,0.1,0.001,'µg/l','Rodiklio ribinė vertė taikoma monomero likučių koncentracijai geriamajame vandenyje, apskaičiuotai pagal geriamojo vandens ruošimo ir tiekimo priemonių specifikacijas, kuriose nurodyta, kiek daugiausia jo išsiskiria iš atitinkamo polimero, besiliečiančio su geriamuoju vandeniu.'),
	(19,2,'CAS_16984-48-8','Fluoridas ','B',0,1.5,0.01,'mg/l ','̶'),
	(20,2,'EEA_33-86-3','Haloacetinės rūgštys','B',0,60,1,'µg/l','Rodiklio ribinė vertė taikoma penkių cheminių medžiagų - monochloracto rūgšties, dichloracto rūgšties, trichloracto rūgšties, monobromacto rūgšties ir dibromacto rūgšties verčių sumai. Rodiklis matuojamas tik tuo atveju, jei geriamajam vandeniui dezinfekuoti naudojami metodai, kurių metu gali susidaryti haloacetinės rūgštys. Rodiklio ribinė vertė taikoma nuo 2026 m. sausio 12 d. Iki tos datos nereikalaujama vykdyti šio rodiklio stebėsenos.'),
	(21,2,'CAS_7439-92-1','Švinas','B',0,10,0.01,'µg/l ','Iki 2036 m. sausio 12 d. taikoma rodiklio ribinė vertė – 10,0 µg/l, po šios datos - 5,0 µg/l.'),
	(22,2,'CAS_7439-97-6','Gyvsidabris ','B',0,1,0.01,'µg/l ','̶'),
	(23,2,'CAS_101043-37-2','Mikrocistinas-LR','B',0,1,0.01,'µg/l','Šis rodiklis matuojamas tik tuo atveju, jei yra tikėtinas vandens šaltinio žydėjimas (didėjantis melsvadumblių ląstelių tankis arba galimas vandens žydėjimas). Rodiklio ribinė vertė taikoma nuo 2026 m. sausio 12 d. Iki tos datos nereikalaujama vykdyti šio rodiklio stebėsenos.'),
	(24,2,'CAS_7440-02-0','Nikelis ','B',0,20,0.1,'µg/l ','̶'),
	(25,2,'CAS_14797-55-8','Nitratas','B',0,50,1,'mg/l ','Turi būti laikomasi sąlygos [nitratas]/50 + [nitritas]/3 ≤ 1 (laužtiniuose skliaustuose įrašomos nitratui (NO3) ir nitritui (NO2) nustatytos koncentracijos mg/l) ir  nitritams nustatytos 0,10 mg/l rodiklio ribinės vertės būtų laikomasi geriamajame vandenyje, ištekančiame iš geriamojo vandens ruošimo įrenginių.'),
	(26,2,'CAS_14797-65-0','Nitritas (vandenyje ištekančiame iš geriamojo vandens ruošimo įrenginių)','B',0,0.1,0.01,'mg/l','Turi būti laikomasi  nitritams nustatytos 0,10 mg/l rodiklio ribinės vertės  geriamajame vandenyje, ištekančiame iš vandens ruošimo įrenginių.'),
	(28,2,'VMVT_001','Nitratai / nitritai formulė ','B',0,1,0.01,'skaičius','Turi būti laikomasi sąlygos [nitratas]/50 + [nitritas]/3 ≤ 1 (laužtiniuose skliaustuose įrašomos nitratui (NO₃) ir nitritui (NO₂) nustatytos koncentracijos mg/l).'),
	(29,2,'EEA_34-01-5','Pesticidai','B',0,0.1,0.001,'µg/l','Pesticidai – organiniai insekticidai, organiniai herbicidai, organiniai fungicidai, organiniai nematocidai, organiniai akaricidai, organiniai algicidai, organiniai rodenticidai, organiniai slimicidai, susiję produktai (įskaitant augimo reguliatorius) bei jų metabolitai, kaip apibrėžta 2009 m. spalio 21 d. Europos Parlamento ir Tarybos reglamento (EB) Nr. 1107/2009 dėl augalų apsaugos produktų pateikimo į rinką ir panaikinančio Tarybos direktyvas 79/117/EEB ir 91/414/EEB su visais pakeitimais 3 straipsnio 32 punkte, kurie laikomi reikšmingais geriamojo vandens atžvilgiu. Pesticidų metabolitas turi būti laikomas reikšmingu geriamojo vandens atžvilgiu, jei yra pagrindas laikyti, kad jam būdingos savybės yra panašios į pirminės medžiagos savybes savo pesticidiniu aktyvumu arba, kad jis pats arba kaip virsmo produktas kelia riziką vartotojų sveikatai. 
	Stebėsenos metu reikia kontroliuoti tik tuos pesticidus, kurie iš geriamojo vandens šaltinio gali patekti ar pateko į konkrečius geriamojo vandens tiekimo įrenginius. 
	0,10 μg/l rodiklio ribinė vertė turi būti taikoma kiekvienam atskiram pesticidui, išskyrus šios lentelės 25.1–25.4 papunkčiuose nurodytus pesticidus. 
	Pesticidų nereikšmingų metabolitų stebėsena turi būti vykdoma tik tuo atveju, jei yra atliktas vandens gavybos vietoms skirtų vandens baseinų rizikos vertinimas ir jo rezultatai rodo, kad yra tikėtinas tų medžiagų buvimas konkrečioje geriamojo vandens tiekimo sistemoje.'),
	(30,2,'CAS_309-00-2','Aldrinas','B',0,0.03,0.001,'µg/l','̶'),
	(37,2,'EEA_33-62-5','Policikliniai aromatiniai angliavandeniliai','B',0,0.1,0.001,'µg/l ','Rodiklio ribinė vertė taikoma benzo-b-fluoranteno, benzo-k-fluoranteno, benzo-ghi-perileno ir indeno (1,2,3-cd) pireno verčių sumai.'),
	(38,2,'CAS_7782-49-2','Selenas','B',0,20,0.1,'µg/l ','Turi būti taikoma 30 μg/l rodiklio ribinė vertė regionuose, kuriuose dėl geologinių sąlygų galėtų susidaryti didelės seleno koncentracijos požeminiame vandenyje.'),
	(39,2,'EEA_33-42-1','Tetrachloreteno ir trichloreteno suma','B',0,10,0.1,'µg/l','Rodiklio ribinė vertė taikoma abiejų cheminių medžiagų  verčių sumai.'),
	(40,2,'EEA_33-43-2','Trihalometanų suma','B',0,100,1,'µg/l ','Rodiklio ribinė vertė taikoma chloroformo, bromoformo, dibromchlormetano ir bromdichlormetano verčių sumai. Turi būti siekiama kuo mažesnių geriamojo vandens dezinfekcijos metu susidarančių antrinių junginių – bromato ir trihalometanų sumos – verčių, nemažinant dezinfekcijos veiksmingumo.'),
	(41,2,'CAS_7440-61-1','Uranas','B',0,30,0.1,'µg/l','Rodiklio ribinė vertė taikoma nuo 2026 m. sausio 12 d. Iki tos datos nereikalaujama vykdyti šio rodiklio stebėsenos.'),
	(42,2,'CAS_75-01-4','Vinilo chloridas','B',0,0.5,0.01,'µg/l ','Rodiklio ribinė vertė taikoma monomero likučių koncentracijai geriamajame vandenyje, apskaičiuotai pagal geriamojo vandens ruošimo ir tiekimo priemonių specifikacijas, kuriose nurodyta, kiek daugiausia jo išsiskiria iš atitinkamo polimero, besiliečiančio su geriamuoju vandeniu.'),
	(61,4,'VMVT_009','Radonas','Rad',0,100,1,'Bq/l',''),
	(62,4,'VMVT_010','Tritis','Rad',0,100,1,'Bq/l',''),
	(45,3,'CAS_16887-00-6','Chloridas ','B',0,250,1,'mg/l ','Geriamasis vanduo neturi būti korozinis'),
	(48,3,'VMVT_005','Savitasis elektrinis laidis','A',0,2500,1,'µS cm⁻¹ 20⁰C ','Geriamasis vanduo neturi būti agresyvus'),
	(63,4,'VMVT_011','Indikacinė dozė','Rad',0,0.1,0.01,'mSv ',''),
	(64,4,'VMVT_012','Visuminis alfa aktyvumas','Rad',0,0.1,0.0001,'Bq/l','Jei visuminio alfa aktyvumo koncentracija yra mažesnė nei 0,1 Bq/l, o visuminio beta aktyvumo koncentracija yra mažesnė nei 1,0 Bq/l, daroma prielaida, kad indikacinė dozė yra mažesnė už 0,1 mSv radiologinio rodiklio vertę.'),
	(65,4,'VMVT_013','Visuminis beta aktyvumas','Rad',0,1,0.001,'Bq/l','Jei visuminio alfa aktyvumo koncentracija yra mažesnė nei 0,1 Bq/l, o visuminio beta aktyvumo koncentracija yra mažesnė nei 1,0 Bq/l, daroma prielaida, kad indikacinė dozė yra mažesnė už 0,1 mSv radiologinio rodiklio vertę.'),
	(66,4,'VMVT_014','U-238','',0,3,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(67,4,'VMVT_015','U-234','',0,2.8,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(68,4,'VMVT_016','Ra-226','',0,0.5,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(69,4,'VMVT_017','Ra-228','',0,0.2,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(70,4,'VMVT_018','Pb-210','',0,0.2,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(71,4,'VMVT_019','Po-210','',0,0.1,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(72,4,'VMVT_020','C-14','',0,240,0.1,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(73,4,'VMVT_021','Sr-90','',0,4.9,0.001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(74,4,'VMVT_022','Pu-239 / Pu-240','',0,0.6,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(75,4,'VMVT_023','Am-241','',0,0.7,0.0001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(76,4,'VMVT_024','Co-60','',0,40,0.001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(77,4,'VMVT_025','Cs-134','',0,7.2,0.001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(78,4,'VMVT_026','Cs-137','',0,11,0.001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(79,4,'VMVT_027','I-131 ','',0,6.2,0.001,'Bq/l','Išvestoji radionuklido aktyvumo koncentracija geriamajame vandenyje.'),
	(80,5,'VMVT_028','Laisvas chloro kiekis','A',0,5,0.01,'mg/l','Nereglamentuotas. PSO nustatyta vertė -5 mg/l.'),
	(81,6,'VMVT_029','Drumstumas vandens tiekimo įmonėje (Kas savaitę)','Dr1',1,1,1,'T/N','Drumstumas geriamojo vandens tiekimo įmonėje. 0,3 nefelometrinių drumstumo vienetų (NTU) 95 % mėginių atveju ir nė vienas negali viršyti 1 NTU.
	Mėginių ėmimo dažnumas: Kas savaitę'),
	(82,6,'VMVT_030','Drumstumas vandens tiekimo įmonėje (Kasdien)','Dr2',1,1,1,'T/N','Drumstumas geriamojo vandens tiekimo įmonėje. 0,3 nefelometrinių drumstumo vienetų (NTU) 95 % mėginių atveju ir nė vienas negali viršyti 1 NTU.
	Mėginių ėmimo dažnumas: Kasdien'),
	(83,6,'VMVT_031','Drumstumas vandens tiekimo įmonėje (Nuolat)','Dr3',1,1,1,'T/N','Drumstumas geriamojo vandens tiekimo įmonėje. 0,3 nefelometrinių drumstumo vienetų (NTU) 95 % mėginių atveju ir nė vienas negali viršyti 1 NTU.
	Mėginių ėmimo dažnumas: Nuolat'),
	(84,6,'VMVT_032','Somatiniai kolifagai','',0,50,0.01,'FPU/100ml','Fagų formuojami vienetai (PFU)/100 ml. Taikoma neparuoštam vandeniui.'),
	(27,2,'CAS_14797-65-0','Nitritas (atitikties vietoje)','B',0,0.5,0.01,'mg/l',''),
	(43,3,'CAS_7429-90-5','Aliuminis ','B',0,200,1,'µg/l ','̶'),
	(44,3,'CAS_14798-03-9','Amonis ','B',0,0.5,0.001,'mg/l ','̶'),
	(46,3,'EEA_15-03-2','Lūžinės klostridijos (Clostridium perfringens) ir jų sporos','B',0,0,1,'skaičius/100ml','Tiriama tuo atveju, jei to poreikį rodo rizikos vertinimo rezultatai'),
	(47,3,'VMVT_004','Spalva','A',1,1,1,'T/N','Priimtina vartotojams ir neturinti nebūdingų pokyčių arba neviršija 30mg/l Pt (kai bangos ilgis 410nm)'),
	(49,3,'EEA_3152-01-0','Vandenilio jonų arba pH vertė','A',6.5,9.5,0.1,'pH vienetai ','Negazuoto fasuojamojo vandens atveju minimali pH vertė gali būti sumažinta iki 4,5 pH vienetų. Fasuojamojo geriamojo vandens, kuris yra natūraliai arba dirbtinai prisotintas anglies dioksido, minimali pH vertė gali būti mažesnė.'),
	(50,3,'CAS_7439-89-6','Geležis','B',0,200,1,'µg/l ','̶'),
	(51,3,'CAS_7439-96-5','Manganas ','B',0,50,1,'µg/l ','̶'),
	(52,3,'VMVT_006','Kvapas','A',1,1,1,'T/N','Priimtinas vartotojams ir neturintis nebūdingų pokyčių'),
	(53,3,'EEA_3133-07-1','Permanganato indeksas / oksiduojamumas','B',0,5,0.1,'mg/l O2','Šio rodiklio geriamajame vandenyje tirti nereikia, jeigu yra tiriama bendrosios organinės anglies (TOC) rodiklio vertė.'),
	(54,3,'CAS_18785-72-3','Sulfatas ','B',0,250,1,'mg/l ','Geriamasis vanduo neturi būti korozinis.'),
	(55,3,'CAS_7440-23-5','Natris ','B',0,200,1,'mg/l ','̶'),
	(56,3,'VMVT_007','Skonis','A',1,1,1,'T/N','Priimtinas vartotojams ir neturintis nebūdingų pokyčių.'),
	(57,3,'VMVT_008','Kolonijas sudarantys vienetai 22 °C temperatūroje ','A',0,0,1,'T/N','Matavimo vienetas - skaičius/1 ml. Vertinama, ar nėra nebūdingų pokyčių'),
	(58,3,'EEA_15-04-3','Koliforminės bakterijos','A',0,0,1,'skaičius/100ml','Fasuojamajam geriamajam vandeniui taikomas matavimo vienetas – skaičius/250 ml vandens.'),
	(59,3,'EEA_3133-06-0','Bendroji organinė anglis (TOC) ','B',0,0,1,'T/N','Matavimo vienetas - mg/l. Vertinama, ar nėra nebūdingų žymių pokyčių. Tiriama tik tuo atveju, jeigu geriamojo vandens tiekimo objekto teritorijai per parą tiekiama daugiau kaip 10 000 m³ vandens.'),
	(60,3,'EEA_3112-01-4','Drumstumas','A',1,1,1,'T/N','Priimtinas vartotojams ir neturintis nebūdingų pokyčių  arba neviršija 4 nefelometrinių drumstumo vienetų (NTU).');



INSERT INTO g9.stebesenos (stb_rodiklis,stb_stebesenos) VALUES
	(1,1),(2,1),(3,1),(4,1),(5,1),(6,1),(7,1),(8,1),(9,1),(10,1),(11,1),(12,1),(13,1),(14,1),(15,1),(16,1),(17,1),(18,1),(19,1),(20,1),(21,1),(22,1),(23,1),(24,1),(25,1),(26,1),(27,1),(28,1),
	(29,1),(30,1),(31,1),(32,1),(33,1),(34,1),(35,1),(36,1),(37,1),(38,1),(39,1),(40,1),(41,1),(42,1),(43,1),(44,1),(45,1),(46,1),(47,1),(48,1),(49,1),(50,1),(51,1),(52,1),(53,1),(54,1),(55,1),
	(56,1),(57,1),(58,1),(59,1),(60,1),(61,1),(62,1),(63,1),(64,1),(65,1),(66,1),(67,1),(68,1),(69,1),(70,1),(71,1),(72,1),(73,1),(74,1),(75,1),(76,1),(77,1),(78,1),(79,1),(80,1),(1,2),(2,2),
	(3,2),(4,2),(5,2),(6,2),(7,2),(8,2),(9,2),(10,2),(11,2),(12,2),(13,2),(14,2),(15,2),(16,2),(17,2),(18,2),(19,2),(20,2),(21,2),(22,2),(23,2),(24,2),(25,2),(26,2),(27,2),(28,2),(29,2),(30,2),
	(31,2),(32,2),(33,2),(34,2),(35,2),(36,2),(37,2),(38,2),(39,2),(40,2),(41,2),(42,2),(43,2),(44,2),(45,2),(46,2),(47,2),(48,2),(49,2),(50,2),(51,2),(52,2),(53,2),(54,2),(55,2),(56,2),(57,2),
	(58,2),(59,2),(60,2),(61,2),(62,2),(63,2),(64,2),(65,2),(66,2),(67,2),(68,2),(69,2),(70,2),(71,2),(72,2),(73,2),(74,2),(75,2),(76,2),(77,2),(78,2),(79,2),(80,2),(81,2),(82,2),(83,2),(84,2);



INSERT INTO g9.gvts (vkl_id,vkl_ja,vkl_title,vkl_saviv,vkl_adresas,vkl_gvtot) VALUES
	(10000001,100000001,'Vilniaus Šeškinės kombinatas','Vilniaus m.','Vilnius, Šeškinės g. 105a','LT-10000001GVTOT'),
	(10000002,100000001,'Vilniaus Antroji','Vilniaus m.','Vilnius, Elekrinės g. 99b','LT-10000002GVTOT'),
	(10000003,100000001,'Vilniaus Turniškių','Vilniaus m.','Vilnius, Pavilnio g. 1a','LT-10000003GVTOT'),
	(10000004,100000002,'Kauno Eigulių','Kauno m.','Kaunas, Kalantos g. 123"','LT-10000004GVTOT'),
	(10000005,100000002,'Kauno Kleboniškio','Kauno m.','Kaunas, Kleboniškio g. 16f','LT-10000005GVTOT'),
	(10000006,100000002,'Kauno Vičiūnų','Kauno m.','Kaunas, Pavilnio g. 1a','LT-10000006GVTOT');

INSERT INTO jar.data (ja_kodas,ja_pavadinimas,adresas) VALUES
	(100000001,'Vilniaus vandenų tiekiėjas','Vilniaus m., Vilniaus g. 10'),
	(100000002,'Kauno vandenų tiekiėjas','Kauno m., Kauno g. 15');




WITH stb as(SELECT lkp_num stb FROM g9.lookup where lkp_group='Stebesenos'),
	yrs as (SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP) yrs, 1 sts UNION SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP)-1 yrs, 2 sts UNION SELECT EXTRACT(YEAR FROM CURRENT_TIMESTAMP)-2 yrs, 3 sts)
INSERT INTO g9.deklaravimas (dkl_status,dkl_metai,dkl_stebesena,dkl_gvts)
SELECT yrs.sts,yrs.yrs,stb.stb,vkl_id FROM yrs,stb,g9.gvts



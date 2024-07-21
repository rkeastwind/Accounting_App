DELETE FROM "main"."map_file" WHERE opt_no='pro_status';
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('pro_status', '結帳狀態', '0', '未結帳', ' ', ' ', '0');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('pro_status', '結帳狀態', '1', '已結帳', ' ', ' ', '1');

CREATE TABLE "bas_user" (
"user_id" TEXT NOT NULL,
"name" TEXT NOT NULL,
"password" TEXT NOT NULL,
"role_id" TEXT NOT NULL,
"dept_id" TEXT NOT NULL,
"enabled" INTEGER NOT NULL,
"loguser" TEXT,
"logtime" TEXT,
PRIMARY KEY("user_id")
);

CREATE TABLE "bas_dept" (
"dept_id" TEXT NOT NULL,
"dept_name" TEXT NOT NULL,
PRIMARY KEY("dept_id")
);

CREATE TABLE "bas_role" (
"role_id" TEXT NOT NULL,
"role_name" TEXT NOT NULL,
PRIMARY KEY("role_id")
);

CREATE TABLE "bas_menu" (
"menu_id" TEXT NOT NULL,
"parent_id" TEXT NOT NULL,
"level" INTEGER NOT NULL DEFAULT 0,
"order_by" INTEGER NOT NULL DEFAULT 0,
"menu_type" INTEGER NOT NULL,
"menu_title" TEXT NOT NULL,
"icon_path" TEXT NOT NULL,
"page_url" TEXT NOT NULL,
"page_para" TEXT NOT NULL,
PRIMARY KEY("menu_id")
);

CREATE TABLE "bas_role_permission" (
"role_id" TEXT NOT NULL,
"menu_id" TEXT NOT NULL,
"cmd_Qurey" INTEGER NOT NULL,
"cmd_Add" INTEGER NOT NULL,
"cmd_Edit" INTEGER NOT NULL,
"cmd_Delete" INTEGER NOT NULL,
"cmd_Export" INTEGER NOT NULL,
PRIMARY KEY("role_id","menu_id")
);

DELETE FROM "main"."bas_role";
INSERT INTO "main"."bas_role" ("role_id", "role_name") VALUES ('r0', '系統管理員');
INSERT INTO "main"."bas_role" ("role_id", "role_name") VALUES ('r1', '帳務負責人');

DELETE FROM "main"."bas_user";
INSERT INTO "main"."bas_user" ("user_id", "name", "password", "role_id", "dept_id", "enabled", "loguser", "logtime") VALUES ('admin', '系統管理員', 'admin0000', 'r0', 'd0', '1', 'sys', DATETIME('now'));

DELETE FROM "main"."bas_dept";
INSERT INTO "main"."bas_dept" ("dept_id", "dept_name") VALUES ('d0','景美禮拜堂');
INSERT INTO "main"."bas_dept" ("dept_id", "dept_name") VALUES ('d1','可愛團契');

DELETE FROM "main"."bas_menu";
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('A001', '0', '1', '1', '0', '交易', '2689', '', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('A002', 'A001', '2', '1', '1', '一般交易', '2689', 'Accounting_App.Form.Form_tra_trade', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('A003', 'A001', '2', '2', '1', '銀行存提', '2689', 'Accounting_App.Form.Form_tra_bankdeal', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('A004', 'A001', '2', '3', '1', '結帳', '2689', 'Accounting_App.Form.Form_pro_date', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('B001', '0', '1', '1', '0', '報表', '6203', '', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('B002', 'B001', '2', '1', '1', '交易明細表', '6203', 'Accounting_App.Form.Form_rpt_R001', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('B003', 'B001', '2', '2', '1', '庫存明細表', '6203', 'Accounting_App.Form.Form_rpt_R002', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('B004', 'B001', '2', '3', '1', '經常費收支表', '6203', 'Accounting_App.Form.Form_rpt_T001', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('B005', 'B001', '2', '4', '1', '會計師財務報表', '6203', 'Accounting_App.Form.Form_rpt_T002', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C001', '0', '1', '1', '0', '設定', '1881', '', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C002', 'C001', '2', '1', '1', '密碼修改', '1881', 'Accounting_App.Form.Form_chg_pwd', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C003', 'C001', '2', '2', '1', '帳號管理', '1881', 'Accounting_App.Form.Form_bas_user', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C004', 'C001', '2', '3', '1', '帳冊基本資料維護', '1881', 'Accounting_App.Form.Form_book_base', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C005', 'C001', '2', '4', '1', '交易備註預設值維護', '1881', 'Accounting_App.Form.Form_tra_mast_memodef', '');
INSERT INTO "main"."bas_menu" ("menu_id", "parent_id", "level", "order_by", "menu_type", "menu_title", "icon_path", "page_url", "page_para") VALUES ('C009', 'C001', '2', '9', '1', '管理員命令工具', '1881', 'Accounting_App.Form.Form_sql_repair_tool', '');

DELETE FROM "main"."bas_role_permission";
INSERT INTO "main"."bas_role_permission"
select
'r0',menu_id,1,1,1,1,1
from "main"."bas_menu";

INSERT INTO "main"."bas_role_permission"
select
'r1',menu_id,1,1,1,1,1
from "main"."bas_menu";

SELECT
a.*,
b.role_id, b.cmd_Qurey, b.cmd_Add, b.cmd_Edit, b.cmd_Delete, b.cmd_Export
FROM bas_menu a
INNER JOIN bas_role_permission b ON a.menu_id = b.menu_id
WHERE b.role_id = 'r0'
ORDER BY parent_id
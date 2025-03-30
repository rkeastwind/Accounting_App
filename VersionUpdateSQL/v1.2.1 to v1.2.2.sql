DELETE FROM "main"."map_file" where opt_no in ('book_type','pro_status');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('AC', '會計科目', 'B03', '其他收入', '', '', '3');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('book_type', '帳冊類型', '0', '總帳冊', '', '', '0');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('book_type', '帳冊類型', '1', '現金帳冊', '', '', '1');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('book_type', '帳冊類型', '2', '銀存帳冊', '', '', '2');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('pro_status', '結帳狀態', '0', '未結帳', ' ', ' ', '0');
INSERT INTO "main"."map_file" ("opt_no", "opt_name", "item", "item_name", "memo1", "memo2", "order_by") VALUES ('pro_status', '結帳狀態', '1', '已結帳', ' ', ' ', '1');
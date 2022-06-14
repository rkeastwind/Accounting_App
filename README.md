<img src="https://github.com/rkeastwind/Accounting_App/raw/master/Accounting_App/images/MainIcon.png" width = "10%" align=center />

# Accounting_App <小型帳務系統>

提供持有「銀行存款」與「現金」的使用者作帳，產出報表。

## Indroduction

結帳流程：輸入一般交易 >> 輸入銀行存提 >> 結帳 >> 列印報表

1.	交易：主要分為「一般交易」與「銀行存提」，前者為一般性的收入支出，只記錄單一方向的交易，後者為不同帳本間的調撥，記錄雙向交易，例如銀行提款，為銀存帳本支出，現金帳本收入。
2.	結帳方式：為「月結」，每個月結帳一次。
3.	報表：分為「系統報表」及「呈報報表」。
4.	管理員命令工具：提供管理員下SQLite指令處理問題，Select或異動皆可。

## Downloads

<table>
  <tr>
    <td>
      <strong>Latest Release: v1.0.1</strong>
    </td>
    <td>
      <a href="https://github.com/rkeastwind/Accounting_App/releases/latest">[ Download ]</a><br />
      <a href="https://github.com/rkeastwind/Accounting_App/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;">
        <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/downloads/rkeastwind/Accounting_App/latest/total?color=%2300cc99"></a>
    </td>
  </tr>
  <tr>
    <td>
      <strong>Specifications</strong>
    </td>
    <td>
      <a href="https://github.com/rkeastwind/Accounting_App/raw/master/小型帳務軟體使用說明書.docx">[ 小型帳務軟體使用說明書.docx ]</a><br />     
    </td>
  </tr>
</table>

## Development

- 使用 Visual Studio Community 2019
- 前端使用WPF用戶端應用程式
- 後端使用SQLite搭配Dapper實作

資料庫路徑可於App.config修改

```C#
  <appSettings>
    <add key="DBPath" value=".\Accounting_App.db" />
  </appSettings>
```

## License

![GitHub User's stars](https://img.shields.io/badge/Copyright%40-Rick%20Lin-blue?style=?style=plastic&logo=GitHub)

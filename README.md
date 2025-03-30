<img src="https://github.com/rkeastwind/Accounting_App/raw/master/Accounting_App/images/MainIcon.png" width = "10%" align=center />

# Accounting_App <小型帳務系統>

提供持有「銀行存款」與「現金」的使用者作帳，產出報表。

***目前會計科目為「教會」所使用，要修改請參考下方說明，或請洽本人***

## Indroduction

自己從事金融業「投資會計帳務軟體」開發已有5年以上的時間，此時重新接任「財團法人基督教會景美禮拜堂」青年團契會計一職，發現目前「對內報表」及「給會計師的報表」有相似之處，且各分支單位為手工作帳，我認為可以開發一個小型帳務軟體，讓各分支單位在自己電腦操作，只要輸入一次交易，就可因應不同需求產生報表，改善重工的問題，歷史交易也可追朔，將來需求有變動，也可由我這邊發佈新的程式版本，於是經過幾次跟各分支單位的訪談，評估可行後，歷時兩個月左右，軟體就此誕生。

**結帳流程：輸入一般交易 >> 輸入銀行存提 >> 結帳 >> 列印報表**

1.	交易：主要分為「一般交易」與「銀行存提」，前者為一般性的收入支出，只記錄單一方向的交易，後者為不同帳本間的調撥，記錄雙向交易，例如銀行提款，為銀存帳本支出，現金帳本收入。
2.	結帳方式：為「月結」，每個月結帳一次。
3.	報表：分為「系統報表」及「呈報報表」。
4.	管理員命令工具：提供管理員下SQLite指令處理問題，Select或異動皆可。

## Downloads

<table>
  <tr>
    <td>
      <strong>Latest Release</strong>
    </td>
    <td>
      <a href="https://github.com/rkeastwind/Accounting_App/releases/latest">[ Download ]</a><br />
      <a href="https://github.com/rkeastwind/Accounting_App/releases/latest" rel="nofollow" style="vertical-align: -webkit-baseline-middle;">
        <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/downloads/rkeastwind/Accounting_App/latest/total?color=Lime"></a>
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

- 使用 Visual Studio Community 2022
- 前端使用WPF用戶端應用程式，搭配MaterialDesignInXaml套版
- 後端使用SQLite搭配Dapper實作

資料庫路徑可於App.config修改

```C#
  <appSettings>
    <add key="DBPath" value=".\Accounting_App.db" />
  </appSettings>
```

會計科目清單在下方，A開頭是支出面，B開頭是收入面

```SQL
    select * from map_file where opt_no = 'AC'
```

## License

![GitHub User's stars](https://img.shields.io/badge/Copyright%40-Rick%20Lin-blue?style=?style=plastic&logo=GitHub)

# Personalhandboksassistent

Nu har Kalle varit i farten igen, och Kalle Anka AB ska skapa ett modernt system för företag att använda internt för sin personal. Specifikationen har följande krav:

Det ska finnas ett admin-gränssnitt där:
  
- Nya interna dokument kan laddas upp
- Existerande dokument kan ersättas eller tas bort

Olika typer av dokument kan laddas upp, exempelvis:
- PDF
- Word
- Markdown
- Ren text

Det ska finnas en AI-assistent som kan besvara personalfrågor baserat på de dokument som är uppladdade
Assistenten får inte hitta på saker
Assistenten ska endast besvara personal-relaterade frågor
Förändringar i dokumentstrukturen ska speglas i assistentens svar utan att kräva omstarter av server och utan att Azure-administratörer ska behöva manuellt omberäkna indexer


Det är önskvärt, men inte ett krav för denna första MVP-version, att det finns:

  - Tydliga markörer från assistenten varifrån informationen kommer, med möjlighet att hoppa till det dokumentet (och gärna den platsen) där informationen fanns
  - Fungerande chat-historik för att möjliggöra följdfrågor
  - Ett riktigt inloggningssystem, framförallt för att nå administrationsgränssnittet
  - Information spaltas upp på ett snyggt sätt i chatten, med riktiga tabeller, punktlistor och dylikt vid behov, precis som i GPT
  - Streaming av AI:ns svar, dvs att text dyker upp ord för ord i stället för att sitta och vänta tills hela svaret färdigt
  Mobilanpassning


Det må vara en kort laboration på bara en vecka, men försök hålla designmönster och agil projektledning i åtanke. Riktiga GIT-grenar och PR:s är en väldigt bra vana!

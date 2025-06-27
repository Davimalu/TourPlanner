# TourPlanner - Final HandIn
Karlheinz Lunatschek & David Zeugner

## Architecture
Wie in der Spezifikation für dieses Projekt gefordert, handelt es sich beim TourPlanner um eine Windows Presentation Foundation (WPF) GUI-Anwendung, die das `MVVM-Pattern` (Model-View-ViewModel) verwendet.
Die Anwendung implementiert zudem die `Layered Architecture`, um bestimmte Geschäftsbereiche der Anwendung korrekt von anderen zu trennen. So sind Anfragen an externe Services (bspw. die REST-API, OpenRouteService API, etc.) in den Data Access Layer (DAL) ausgelagert.
Die UI-Komponenten sind (wie es auch MVVM vorschreibt) von der restlichen Anwendung getrennt und unabhängig; die ViewModels implementieren die zur UI gehörigen Logik, ohne direkt von ihr abhängig zu sein, sodass sie auch mit anderen UI-Frameworks verwendet werden könnten; die Business Logic für die Verarbeitung der Daten und andere Operationen befindet sich ebenfalls in einem eigenen Layer.
Parallel dazu existieren Services, wie bspw. die Konfigurationsverwaltung `TourPlannerConfig.cs`, die sämtliche Layer überspannt.

Die Logik für die Persistenz der Daten ist sogar in eine andere Solution `TourPlanner.RestServer` ausgelagert. Die Frontend Applikation kommuniziert über eine Restful-API mit dem Backend, das das Speichern/Updaten/Löschen der Daten in einer PostgreSQL Datenbank übernimmt.
Die Model-Klassen sind zudem ebenfalls in eine eigenen Solution `TourPlanner.Model` ausgelagert und werden sowohl von `TourPlanner` als auch von `TourPlanner.RestServer` verwendet.

### Einschränkungen / Kompromisse
Wir haben einige der von uns uns soeben genannten Best Practices und Design Patterns (welche genau wir implementiert haben, wird später noch erwähnt) an einigen Stellen bewusst (an manchen Stellen sicher auch unbewusst) verletzt. So implementieren bspw. einige Model-Klassen in `TourPlanner.Model` `INotifyPropertyChanged` und damit zu gewissen Teilen auch Business Logic. Dies war eine pragmatische Entscheidung, um zu vermeiden, dass die ViewModels sämtliche Properties der Models reimplementieren müssen, was zu sehr viel Duplicate Code geführt hätte.

Die CodeBehind Datei von `Map.xaml` enthält Business Logic für die Initialisierung des WebViews und der Services, die es dann managen (Verletzung des MVVM Patterns). Diese Logik aus dem Code Behind auszulagern würde zu einem massiven Anstieg der Komplexität der Applikation bei geringem bis keinen Nutzen führen, da die Logik für die Initialisierung von bspw. WebView2 so spezifisch zum verwendeten UI-Framework ist, dass diese ohnehin nicht an anderer Stelle wiederverwertet werden kann.

### Besonderheiten / Herausforderungen
Eine große Herausforderung war es, die Logik zur Kontrolle des WebViews (und damit in weiterer Folge der Map) unter Einhaltung des `MVVM-Patterns` zu implementieren. Um mit der Map zu interagieren (die sich in einem WebView2 befindet) müssen bestimmte Methoden von `Microsoft.Web.WebView2` aufgerufen werden (bspw. `EnsureCoreWebView2Async`, `ExecuteScriptAsync`, `CapturePreviewAsync`, etc.). Die Best Practices verbieten jedoch, dass wir in unserem ViewModel direkte Abhängigkeiten zu diesen Komponenten haben sollten und die Logik sollte auch nicht im Code Behind File implementiert werden.
Wir haben daher eine Service-Klasse `WebViewService.cs` implementiert, die das `IWebViewService` implementiert und Methoden bereitstellt, um mit dem WebView2 in der View zu interagieren. Andere Klassen können dieses Interface dann als Abstraktion verwenden, um Befehle an das WebView zu senden ohne direkt von ihm abhängig zu sein. 
Wir sind uns bewusst, dass auch diese Lösung nicht vollkommen sauber ist. Eine Service-Klasse sollte eigentlich keine Abhängigkeit zu UI-Komponenten wie dem WebView haben - um diese Abhänhgigkeit zu lösen hätten wir theoretisch eine weitere Abstraktion für die WebView2 Control einführen können und `WebViewServive.cs` dann von dieser abhängig machen. In unseren Augen hätte das aber sehr viel unnötige Komplexität eingeführt und kaum zusätzlichen Nutzen, da `WebViewService.cs` ohnehin so spezifisch auf die WebView2 Komponenten ausgerichtet ist, dass man auch eine Abhängigkeit argumentieren kann.

Ein Problem ähnlicher Natur war, dass wir in manchen Fällen in den ViewModels Funktionalität von WPF benötigten (neue Fenster wie `EditTour(Log)Window` spawnen, `MessageBox`es erstellen, Fenster wieder schließen, etc.). Allerdings verbietet das MVVM Pattern eine direkte Abhängigkeit der ViewModels von der View. Aus diesem Grund haben wir einen `WpfService.cs` implementiert, der mithilfe des `IWpfService.cs` Interfaces eine Abstraktionsebene für unsere ViewModels und auch andere Klassen darstellt.
Diese Klasse ist somit direkt von WPF (`System.Windows`) abhängig (was auch nicht unbedingt Best Practices entspricht - da auch hier die Funktionalität wieder vollkommen spezifisch für WPF ist, haben wir auf eine Abstraktion der WPF-Komponenten verzichtet, um unnötige Komplexität zu vermeiden) und orchestriert verschiedenste UI-Befehle. Andere Klassen können diesen Service dann nutzen, um auf WPF Funktionalitäten zuzugreifen ohne direkt von `System.Windows` abhängig zu sein.

Eine weitere große Herausforderung war es, die Kommunikation zwischen verschiedenen Klassen unter Berücksichtigung der S.O.L.I.D. Kriterien und loser Kupplung zwischen den Komponenten umzusetzen. Darauf werden wir dann im Abschnitt `Design Patterns` zurückkommen.

## Use cases
Actor -> Manage Tours / Import Tours / Export Tours (includes Single Tour Export, Summary Export, Tour Export) / Manage Tour Logs / Search Tours

keine vollständige Auflistung

## UI / UX
MainWindow, EditTourLogWindow, EditTourWindow

## Library Decisions
Nachfolgend wollen wir die relevantesten Libraries / Abhängigkeiten (keine vollständige Auflistung!) in unserem Projekt vorstellen und jeweils erklären, warum diese notwendig sind bzw. wir uns für sie entschieden haben:

- NUnit / NSubstitute
  - Diese Libraries verwenden wir für die Unit-Tests unseres Projekts (erklären wir unten noch etwas genauer). Wir haben uns für diese Library entschieden, da wir beide in der Vergangenheit schon mit NUnit / NSubstitue gearbeitet haben.
- Npgsql.EntityFrameworkCore.PostgreSQL
  - Da wir uns für die Verwendung einer PostgreSQL Datenbank und Entity Framework Core verwendet haben, mussten wir diese Library verwenden, um die beiden miteinander verwenden zu können
- Newtonsoft.Json
  - Wir hätten hier auch die Standard-JSON Library von Microsoft verwenden können, aber laut Internet-Recherchen ist Newtonsoft.Json etwas schneller. Zudem bietet es auch ein paar mehr Funktionen und ist (unserer Meinung nach) komfortabler zu nutzen
- WPF-UI
  - Hierbei handelt es sich um eine UI-Library für WPF, die der Applikation einen modernen (Windows 11 like) Style verleiht. Zudem fügt sich auch einige neue UI-Komponenten hinzu.
  - Wir haben uns für diese Library entschieden, dass uns ihr Design am besten gefiehl, es eine "okaye" Dokumentation gibt und die Library sehr populär und well maintained ist.
- Extended.Wpf.Toolkit
  - Im Rahmen der Entwicklung unserer Applikation ist uns aufgefallen, dass uns ein paar UI-Komponenten, die wir gerne einbauen möchten, fehlen. Dazu zählt bspw. ein DatePicker, der es auch erlaubt eine Uhrzeit anzugeben oder TimeSpan Picker (sprich ein Textfeld, das eine Zeitspanne anzeigt / akzeptiert)
  - ExtendedWpfToolkit fügt eine Reihe von weiteren Controls für WPF hinzu und ist ein extrem bekanntes, gut maintaintes Projekt
- itext7
  - Wir verwenden diese Library, um PDF Dokumente zu generieren. Diese Entscheidung file hauptsächlich deswegen, weil wir beide keine Libraries aus diesem Bereich kannte und iText7 die Library war, die wir in den Präsenzphasen von SWEN durchgemacht haben.
  - Online-Recherchen zeigten auch, dass diese Library gut gepflegt wird und vergleichsweise einfach zu nutzen ist. Wir verwenden sie nur in `PdfService` (haben bewusst keine Abstraktionsebene für diese Library eingebaut, da wir sie ohnehin nur in einem Service verwenden, der sehr spezifisch auf die Library zugeschnitten ist)
- log4net
  - Für diese Library haben wir uns ebenfalls hauptsächlich deshalb entschieden, da sie es war, die wir in der Lehrveranstaltung behandelt haben. Privat habe ich auch schon Erfahrung mit NLog gemacht - im Endeffekt funktionieren alle großen Logging-Libraries nahezu ident.
  - Da wir eine Abstraktionsebene für das Logging eingeführt haben, kann das Logging-Framework auch jederzeit einfach ausgetauscht werden.
- Microsoft.Extensions.*
  - Wir verwenden verschiedenste Abstraktionen, die direkt von Microsoft zur Verfügung gestellt werden. Bspw. für Dependency Injection oder die Konfiguration.
  - Natürlich gibt es hier auch Third Party Lösungen, aber die Lösungen von Microsoft stellen in der .NET Welt quasi den Industry-Standard da und werden gut gepflegt, weshalb wir uns für sie entschieden haben.

## Design Patterns

### MVVM Pattern
Keine Überraschung, daher halten wir diesen Absatz auch recht kurz, aber unsere Applikation implementiert offensichtlich das MVVM Pattern (dessen Implementierung für diese Abgabe ja auch ein K.O. Kriterium ist ).

### Repository Pattern
Zur Kommunikation mit der externen Datenbank verwenden wir in `TourPlanner.RestServer` das Repository Pattern. Die Logik für die Kommunikation mit der Datenbank ist in `TourLogRepository.cs` und `TourRepository.cs` ausgelagert - andere Klassen können dann die Interfaces der Repositories als Abstraktion für Datenbankoperationen verwenden.

### Event Aggregator Pattern
Oben haben wir ja schon erwähnt, dass es eine ziemliche Herausfoderung war, die Kommunikation zwischen den einzelnen Komponenten unserer Anwendung unter Berücksichtigung der Best Practices, die wir einhalten wollen, umzusetzen.

In unserer Applikation gibt es einige Ereignisse, auf die von verschiedenen Komponenten reagiert werden muss. Bspw. wenn der User eine neue Tour auswählt (Map muss geupdated werden, neue Information angezeigt), eine Suche startet (Suche durchgeführt, Tourliste eingeschränkt), neue Touren hinzufügt / löscht (UI Updates, Datenbankoperationen), etc. etc.
Ursprünglich hatten wir für diese Kommunikation das Observer Pattern in seiner primitivsten Form eingesetzt. Bspw. stellte das `TourListViewModel.cs` ein `TourChangedEvent` zur Verfügung, das von anderen abboniert werden konnte, wenn sie darüber informiert werden wollten, wenn der User in der TourList eine andere Tour auswählt. (ähnliches Vorgehen auch für andere Events). Allerdings hat das dazu geführt, dass wir viele Abhängigkeiten der Komponenten untereinander hatten, die eigentlich keinen Nutzen hatten außer den Austausch der Events.
Wir haben daher zunächst als Notlösung eigene Services erstellt, bspw. einen `SelectedTourService.cs`, der stets Informationen über die aktuell vom Nutzer ausgewählte Tour enthielt und von allen an der aktuell selektieren Tour interessierten Komponenten als Abstraktion genutzt werden konnte (um nicht direkt von `TourListViewModel` abhängig zu sein). Allerdings war auch dieser Ansatz nicht optimal, denn mit steigender Anzahl von Events, benötigten wir immer mehr solche Services (bspw. `SearchQueryService`, etc.).

Da wir uns gedacht haben, dass dieses Problem sicherlich schon viele Leute beim Entwicklen von UI Applikationen haben musste, haben wir uns online umgeschaut und sind auf das `EventAggregatorPattern` gestoßen: Wir definieren eine zentrale Klasse (die natürlich auch gegen ein Interface implementiert ist), die das Publishes, Subscriben und Unscubscriben von verschiedensten Events (werden in `TourPlanner.Model` definiert) managed.
So gibt es jetzt ein einheitliches System, das die Kommunikation zwischen verschiedenen Komponenten regelt, ohne dass diese dafür voneinander abhängig sein müssen. Durch die Definition der Event-Klassen in `TourPlanner.Model` können auch die zu übergebenden Daten komplett frei gewählt werden.

### Dependency Injection / Singleton Pattern
Wir verwenden ein Dependency Injection Framework, um die Dependencies für unsere Klassen zentral zu Verwalten und sie ihnen bei Bedarf automatisiert zur Verfügung zu stellen.
Viele dieser Klassen werden dabei als Singleton gemanaged, wodurch auch das Singleton Pattern in unserer Applikation implementiert wird.

### Command Pattern
Wir verwenden das Command Pattern, um das View vom ViewModel zu entkoppeln. Stattdessen binden wir das Command Property der UI-Elemente auf eine Methoden in unserem ViewModel, das das `ICommand` Interface implementiert.
Dazu haben wir `RelayCommand.cs` und (für asynchrone Operationen) `RelayCommandAsync.cs` implementiert. Außerhalb des akademischen Umfelds wäre es vermutlich sinnvoll, eine bereits vorhandene Implementierung zu nutzen anstatt diese selbst zu schreiben.

### Facade Pattern
Das Facade Pattern wird verwendet, um eine einfache Abstraktion für ein komplexes zugrundeliegendes System zur Verfügung stellen. Wir nutzen dieses Pattern bspw. in `WpfService`, `OrsService`, `AiService` oder `PdfService`, indem wir die zugrundeliegenden Library-Funktionen vom Rest der Appliaktion verstecken und ihnen stattdessen mit diesen Klassen (und ihren zugehörigen Interfaces) eine High-Level Abstraktion dafür anbieten.

### Adapter Pattern
Das Adapter Pattern ist ein Design Pattern, das verwendet werden kann, um eine Abstraktionseben zwischen unserem Code und einem externen (oft inkompatiblen) System zur Verfügung zu stellen.
Wir implementieren es im `FileSystemWrapper`, um unseren Klassen eine Abstraktion des Filesystems zur Verfügung zu stellen und auch Unit Tests zu ermöglichen.

## Unit Tests

## Unique Features
AI

Dark Mode
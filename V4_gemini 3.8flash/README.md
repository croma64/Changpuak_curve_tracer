# TANACHAI CURVE TRACER PRO (v2.5 / V4)
### Upgrade Scientifico & Metrologico Completo per Arduino Shield TANACHAI (ETH Quantum Optics / Changpuak.ch V2B2)

---

## 📁 Struttura della Cartella

```text
V4_gemini 3.8flash/
│
├── Guida_Modifiche_Hardware_Curve_Tracer_TANACHAI.pdf # Manuale tecnico completo modifiche HW (termica R7/T1, isolamento USB)
│
├── Firmware/
│   └── TANACHAI_PRO.ino           # Firmware Arduino UNO (v2.5) con trimmed-mean filter e protocollo ad alta velocità
│
├── Software_Source/               # Codice sorgente completo C# .NET 9.0 Windows Forms
│   ├── TanachaiProSuite.csproj
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── AppIcon.ico
│   ├── Models/
│   │   └── MeasurementData.cs     # Modelli dati, preset semiconduttori e classi di misura
│   └── Services/
│       ├── SerialManager.cs       # Gestore seriale deterministico, thread-safe e non bloccante
│       ├── MetrologyEngine.cs     # Motore di calcolo parametri (hFE, hfe, Va, ro, gm, Vf, Zz)
│       └── ExcelExportManager.cs  # Esportatore EPPlus con fogli formattati e grafico Scatter nativo
│
└── Ready_To_Run/                  # Applicazione compilata in Release, 100% pronta all'avvio immediato
    └── TanachaiProSuite.exe       # Doppio clic per avviare il software di laboratorio
```

---

## ⚡ Principali Migliorie e Caratteristiche

1. **Velocità di Scansione Ottimizzata (2–3 secondi):**
   * Sostituiti i vecchi `delay(2000)` con un tempo di assestamento configurabile di default a **8 ms**.
   * Lo sweep da 50 punti $\times$ 5 curve viene completato in circa **2.5 secondi** invece di 8.3 minuti.

2. **Filtro ADC a Media Troncata (*Trimmed Mean* a 32 campioni):**
   * Elimina completamente il rumore di commutazione e i picchi a dente di sega dello step-up MC34063A.
   * Scarta i 4 campioni più alti e i 4 più bassi e media i 24 centrali.

3. **Compensazione Tensione Reale al DUT ($V_{DUT}$):**
   * Calcola in tempo reale la caduta di tensione sullo shunt da $1.0\ \Omega$ per tracciare la reale tensione $V_{CE}$ applicata al componente:
     $$V_{DUT} = V_{set} - I_{meas} \times R_{shunt}$$

4. **Sicurezza Hardware Attiva & Abort Istantaneo (< 1 ms):**
   * Calcolo della potenza istantanea dissipata sul pass-transistor $T_1$ (BC140): se $P > P_{max}$, il ciclo viene interrotto immediatamente.
   * Il pulsante **STOP / ABORT** interrompe lo sweep in tempo reale azzerando all'istante entrambi i DAC a $0\text{ V}$.

5. **Interfaccia Grafica di Precisione (ScottPlot 5):**
   * Asse X in **Volt reali** ($V_{CE}, V_{DS}, V_{DUT}$) e Asse Y in **mA**.
   * Rendering live fluido a 30 FPS.
   * Mirino interattivo (*Crosshair*) con visualizzazione al passaggio del mouse di: $(V, I)$, $I_B$, potenza e $h_{FE}$ puntuale.

6. **Motore Metrologico Avanzato:**
   * Calcola in automatico:
     * $h_{FE}$ (Guadagno Statico DC) adattato all'intervallo reale.
     * $h_{fe}$ (Guadagno Dinamico AC per piccoli segnali) tra curve adiacenti.
     * Tensione di Early ($V_A$) e Resistenza di Uscita ($r_o$) tramite regressione lineare.
     * Transconduttanza ($g_m$) per FET/MOSFET.
     * $V_F$ (1mA e 10mA) e resistenza differenziale per Diodi.
     * Tensione Zener ($V_Z$) e impedenza dinamica ($Z_Z$).

7. **Esportazione Professionale in Excel (.xlsx):**
   * Genera un file Excel a due fogli:
     * **Foglio 1 ("Summary & Metrology"):** Metadati di misura e tabella con tutti i parametri estratti.
     * **Foglio 2 ("Curves Data"):** Tabella completa dei dati grezzi affiancata a un **grafico Scatter nativo a dispersione incorporato**.
   * Esportazione anche in formato CSV e salvataggio immagini ad alta risoluzione (Full HD / 1080p).

---

## 🔌 Connessioni Hardware sui Morsetti (Zoccolo K4 / Pin K6–K9)

| N° Pin | Etichetta | Funzione Circuitale |
| :---: | :---: | :--- |
| **0** | **K6** | Massa di riferimento (GND) |
| **1** | **K7** | Ritorno di corrente (attraversa lo shunt $R_1 = 1.0\ \Omega$ verso GND) |
| **2** | **K8** | Uscita Tensione/Corrente Base o Gate ($0\text{--}27\text{ V}$ tramite $R_{12} = 27\text{ k}\Omega$) |
| **3** | **K9** | Uscita Tensione Collettore o Drain ($0\text{--}27\text{ V}$, fino a $\approx 100\text{ mA}$) |

### Mappatura Componenti:
* **BJT NPN:** Emettitore su **1**, Base su **2**, Collettore su **3**.
* **BJT PNP:** Collettore su **1**, Base su **2**, Emettitore su **3**.
* **MOSFET N-Channel:** Source su **1**, Gate su **2**, Drain su **3**.
* **JFET N-Channel:** Drain su **1**, Gate su **2**, Source su **3**.
* **Diodi / Zener / Varistori:** Anodo su **1**, Catodo su **3** (oppure invertiti per prova inversa); pin **2** non connesso.

---

## 🚀 Istruzioni di Avvio Rapido

1. **Firmware:** Apri `Firmware/TANACHAI_PRO.ino` nell'Arduino IDE e caricalo sulla scheda Arduino UNO.
2. **Hardware:** Collega l'alimentatore esterno $12\text{ V} / 1\text{ A}$ al jack DC di Arduino.
3. **Applicazione:** Entra nella cartella `Ready_To_Run` e avvia `TanachaiProSuite.exe`.
4. Seleziona la porta COM corrispondente (baudrate **115200**), clicca **CONNECT**, seleziona il preset del componente e premi **▶ START SWEEP**.

# Afterlight Kingdom

## Abstract
Il gioco è un platform 3D a scorrimento laterale con rotazione di camera libera, composto da 3 livelli che si differenziano 
per ambientazione fantasy-medievale. Le principali meccaniche consistono in un combat system basato sull'arco con diverse tipologie 
di nemici, puzzle ambientali e abilità di dashing. Il gioco è strutturato su 3 livelli: partirà in una foresta in pieno giorno, per poi spostarsi al di fuori di essa al tramonto, 
con l'obiettivo finale di completare il livello 3 (ambientato di notte), dove si raggiungerà la cima di un castello per reclamare il trono. 
La scelta del titolo deriva in parte da questa transizione fra le varie ore del giorno.

Due NPC avranno il compito di spiegare il world building ed aiutare il giocatore nella comprensione del gameplay.

Il sistema di tutorial è stato diviso in due componenti: una tecnica ed una narrativa non necessariamente mutalmente esclusive.
Mentre la tecnica si occupa di precise informazioni di gameplay (come i comandi), la narrativa cerca di aumentare l'immersione nel mondo di gioco, a volte chiedendo al giocatore di fermarsi e ragionare su come superare l'ostacolo. 

Presente anche un sistema di collezionabili: monete e chiavi. Le chiavi dovranno essere trovate per proseguire in determinati passaggi dei livelli; 
le monete non hanno la mera funzione di collezionabili, ma permetteranno di sbloccare un powerup alla vita durante il terzo livello.

L'arco è il principale strumento a disposizione del giocatore; oltre a ferire i nemici, può essere utilizzato per incastrare le frecce al muro, ed utilizzarle come piattaforma.
Proseguendo nel gioco si otterrà una seconda abilità: il Dash, che permette spostamenti più lunghi.

All'interno del gioco si trovano tre tipi di nemici: uno ranged (mago), uno melee (guerriero) e un nemico AOE (scheletro) tutti guidati da NavMesh.

Il gioco possiede un Save State, per mantenere le informazioni relative alla partita (nemici sconfitti, collezionabili raccolti, porte aperte etc.). Il salvataggio viene aggiornato
ogni volta che il giocatore raggiunge un checkpoint.

Presenti i settings di gioco, che permettono di: attivare e disabilitare il fullscreen, preferenze sulla rotazione di camera e volume (diviso fra VFX e background music).

HUD e UI sono state interamente sviluppate dal gruppo, partendo dalla creazione di un design system per il gioco tramite FIGMA ([reperibile qui](https://www.figma.com/design/pnuxmhyXjGaJILGgRkSXXk/Afterlight-Kingdom?node-id=120-381&t=RO1ZKZ9EHjnZALkL-1)).

Alcune delle animazioni semplici (come la Chest) sono state sviluppate da noi, la maggior parte invece provengono da Mixamo, e sono state utilizzate per i nemici ed il personaggio.

Fra le Shaders da noi sviluppate si trovano: l'effetto visivo del Dash, Il fuoco per le torce del terzo livello, Light Mask per le ombre generate dal fogliame (livello 1).

Non abbiamo inserito skybox, abbiamo piuttosto preferito usare un sistema di Parallasse, diverso per ogni tipo di livello, in modo da dare profondità al mondo di gioco, poiché non è presente alcun terrain.

Tutti gli asset di gioco sono stati scelti per mantenere una grafica low-poly che si addicesse allo stile platform del gioco, anche in modo da rendere più rapido lo sviluppo del level design, prevalentemente formato da tiles.

## Assets
* `Assets/Resources/Sounds` - Suoni (.wav, .mp3 e .FLAC): https://freesounds.org
  * Alcuni dei quali generati da noi tramite ....
* `Assets/Resources/Sprites` 
  * UI (Bottoni, HUD): Creati da noi tramite FIGMA ([Design System](https://www.figma.com/design/pnuxmhyXjGaJILGgRkSXXk/Afterlight-Kingdom?node-id=120-381&t=RO1ZKZ9EHjnZALkL-1)).
  * Icone (Bow, Dash) usate per FIGMA: https://www.svgrepo.com/
* `Assets/Externals` - Contiene tutti gli asset proveniente dallo _Unity Asset Store_
  * Dettagli interni al livello: [Pandazole](https://assetstore.unity.com/packages/3d/props/pandazole-lowpoly-asset-bundle-226938)
  * Tool per l'inserimento di prefab tramite pennelli (all'esterno dei Terrain): [PrefabPainter](https://assetstore.unity.com/packages/tools/painting/prefab-painter-2-61331)
  * Statua powerup della vita per il Livello 3: [IThappy](https://assetstore.unity.com/packages/3d/environments/fantasy/inferno-world-free-low-poly-3d-models-328402)
  * Prefab vari per il level design:
    * [Low Poly Dungeons Lite](https://assetstore.unity.com/packages/3d/environments/dungeons/low-poly-dungeons-lite-177937)
    * [Roadside Tales Free](https://assetstore.unity.com/packages/3d/environments/landscapes/roadside-tales-free-modular-fence-narrative-pack-133601)
    * [GanzSe FREE Weapons](https://assetstore.unity.com/packages/3d/props/weapons/ganzse-free-weapons-fantasy-low-poly-pack-320869)
  * Animazioni per l'arco (presenti anche degli AnimationController, ma non utilizzati): [Human Archer Animations](https://assetstore.unity.com/packages/3d/animations/human-archer-animations-free-335880)
  * Libreria per il motor del personaggio comandato (stessa del Laboratori del corso): [Kinematic Character Motor](https://assetstore.unity.com/packages/tools/physics/kinematic-character-controller-99131)
* `Assets/Mixamo` - Animazioni del personaggio e dei nemici ([Mixamo](https://www.mixamo.com/#/))
* `Assets/Prefabs` - Alcuni dei prefab creati da noi, utilizzando gli Asset sopra elencati.

Repo del progetto: https://github.com/Davoleo/Afterlight-Kingdom

Roccia svolazzante
piazzare AOE enemy
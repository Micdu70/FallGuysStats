# Sommaire

  - [**Tracker Fall Guys "FE" (Frenchy Edition) par Micdu70**](#-tracker-fall-guys-fe-frenchy-edition-par-micdu70)
    - [**Différences entre la "FE" et la Version Officielle**](#différences-entre-la-fe-et-la-version-officielle)
    - [**Téléchargement**](#téléchargement)
    - [**Changelog de la dernière version**](#changelog-de-la-dernière-version)
    - [**Utilisation**](#utilisation)
    - [**FAQ**](#faq)
    - [**Thème**](#thème)
      - [**Thème Clair**](#thème-clair)
      - [**Thème Sombre**](#thème-sombre)
  - [**Langues disponibles**](#langues-disponibles)
  - [**Overlay**](#overlay)
    - [**Raccourcis**](#raccourcis)
    - [**Créer vos propres arrière-plans**](#créer-vos-propres-arrière-plans)
  - [**Profil**](#profil)
    - [**Lier un profil à une émission spécifique**](#lier-un-profil-à-une-émission-spécifique)
    - [**Supprimer des émissions ou déplacer des émissions vers un autre profil**](#supprimer-des-émissions-ou-déplacer-des-émissions-vers-un-autre-profil)
  - [**Copier le code de partage d'une map que vous avez précédemment jouée**](#copier-le-code-de-partage-dune-map-que-vous-avez-précédemment-jouée)
  - [**Changelog complet de la "FE" (Frenchy Edition)**](#changelog-complet-de-la-fe-frenchy-edition)
  - [**Changelog récent de la Version Officielle**](#changelog-récent-de-la-version-officielle)


# [![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/Micdu70/FallGuysStats/blob/master/LICENSE) Tracker Fall Guys "FE" (Frenchy Edition) par Micdu70

**IMPORTANT: Cette application n'a aucune affiliation avec MediaTonic. Les images de Fall Guys sont la propriété de Mediatonic Limited.**

Programme qui permet de récupérer les stats de Fall Guys (via la lecture des logs) pour suivre ce que vous faites en jeu.

INFO: La Version Officielle est disponible ici => https://github.com/ShootMe/FallGuysStats


## Différences entre la "FE" et la Version Officielle

- La "FE" enregistre les stats des manches des émissions abandonnées (quittées prématurément) contrairement à la Version Officielle
- L'overlay de la "FE" fonctionne aussi en mode spectateur contrairement à la Version Officielle
- L'overlay de la "FE" indique le nombre d'haricots qui ont réussi la manche (fini la course/survécu/TO en finale) contrairement à la Version Officielle

- La "FE" ne possède pas d'icône de notification (systray) contrairement à la Version Officielle qui l'a récemment rajoutée
- La "FE" utilise toujours l'ancienne fenêtre de "Configuration" contrairement à la Version Officielle qui possède maintenant des "tuiles"


## Téléchargement

**Dernière version:** `v1.175` ~ 24/05/2023

　　<a href="https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStats.zip">![FallGuysStats.zip](Resources/FallGuysStats-download.svg)</a>
  - Si votre logiciel antivirus bloque l'utilisation du tracker, utilisez alors la version ci-dessous qui ne possède pas la fonction de MAJ automatique.

　　<a href="https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/FallGuysStatsManualUpdate.zip">![FallGuysStats.zip](Resources/FallGuysStatsManualUpdate-download.svg)</a>


## Changelog de la dernière version

{ Correction de bugs de la "FE" }
{ Cette version, bien que basée sur la Version Officielle (v1.174) datant du 24/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
- +Corrigé: Détection de la manche finale "Basketfall" dans l'émission de Groupe [Version Officielle]
- ++Ajouté: Nom des récentes émissions de la nouvelle saison pour la liste des stats [Version Officielle + Ajout "FE"]
- ++Ajouté: Un système de surveillance de l'état (en cours d'exécution ou non) du jeu durant vos parties pour essayer d'enregistrer vos stats en cas de crash du jeu
- ++Ajouté: Renommage du nom des émissions - pour les maps créatives créées par d'autre joueur - par le nom des maps (via l'API de FallGuysDB) [Version Officielle + Modifications "FE"]
- ++Ajouté: Option pour mettre à jour le nom des émissions pour les maps créatives créées par d'autre joueur (via un clique droit sur la manche) [Version Officielle + Modifications "FE"]
- ++Ajouté: Filtre "Période" dans le sous-menu "Stats" pour configurer une période de temps que vous voulez [Version Officielle + Modifications "FE"]


### Il y a 0 bug génant connu dans la v1.175 de la "FE" (Frenchy Edition)


## Utilisation

  - Première fois:
    - Extraire le contenu du fichier zip téléchargé vers un nouveau dossier vide
    - Dans ce nouveau dossier, lancer le tracker
    - Configurer le tracker comme bon vous semble (afficher l'overlay/modifier des options/etc.)
    - Lancer Fall Guys

  - Mise à jour (depuis une Version Officielle ou d'une ancienne version "FE"):
    - Extraire le contenu du fichier zip téléchargé vers le dossier contenant l'ancienne version du tracker à remplacer
	- Lancer la nouvelle version du tracker
    - Configurer le tracker comme bon vous semble (afficher l'overlay/modifier des options/etc.)
    - Lancer Fall Guys


### Fenêtre Principale

![Fenêtre Principale](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindow.png)


### Liste des Stats des Manches

![Stats des Manches](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindow.png)


## FAQ

**Q.1) Que signifie une ligne de couleur grise (thème clair) ou noire (thème sombre) dans la liste des stats des manches ?**

------> Cela signifie que c'est une manche jouée dans une Partie Personnalisée.

**Q.2) Que signifie une ligne de couleur rose claire (thème clair) ou violette sombre (thème sombre) dans la liste des stats des manches ?**

------> Cela signifie que c'est une manche jouée dans une émission abandonnée (quittée prématurément).


## Thème

  - Le tracker supporte actuellement deux thèmes: "Clair" et "Sombre".


### Thème Clair

![Thème Clair - Fenêtre Principale)](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindowLightTheme.png)

![Thème Clair - Stats des Manches)](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindowLightTheme.png)


### Thème Sombre

![Thème Sombre - Fenêtre Principale](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/mainWindowDarkTheme.png)

![Thème Sombre - Stats des Manches](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/levelWindowDarkTheme.png)


## Langues disponibles

  - Le Tracker Fall Guys "FE" supporte les langues suivantes:
    - 🇺🇸 English (Anglais)
    - 🇫🇷 Français
    - 🇰🇷 한국어 (Coréen)
    - 🇯🇵 日本語 (Japonais)
    - 🇨🇳 简体中文 (Chinois Simplifié)


## Overlay

![Overlay](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/overlay.png)


### Raccourcis

  - Cliquer une fois sur l'overlay pour le sélectionner puis:
    - Appuyer sur **'Ctrl + Maj + D'** pour remettre les dimensions par défaut de l'overlay.
    - Appuyer sur la touche **'T'** pour changer la couleur de l'arrière-plan.
    - Appuyer sur la touche **'F'** pour inverser horizontalement l'affichage.
    - Appuyer sur la touche **'P'** pour passer au profil suivant.
    - Appuyer sur les touches des **chiffres situés au dessus des lettres (1 à 9)** pour changer de profil.
    - Maintenir la touche **'Maj' enfoncée et utiliser la molette de votre souris** pour changer de profil.
    - Maintenir la touche **'Maj' enfoncée et utiliser la touche directionnelle 'Droite'/'Bas' ou 'Gauche'/'Haut'** pour changer de profil.
    - Appuyer sur la touche **'C'** pour afficher le nombre de joueurs par support de jeu.
    - Appuyer sur la touche **'R'** pour colorer le nom des manches selon leur type.


### Créer vos propres arrière-plans

![Overlay Modifié](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/customizedOverlay.png)

  - **Prérequis** Lancer au moins une fois le tracker Fall Guys pour obtenir un dossier nommé "Overlay".


  - **Étape 1.** Modifier l'image **background.png** et **tab.png** qui sont dans le dossier "Overlay" (où se trouve le fichier d'exécution du tracker).


  - **Étape 2.** Nommer les images modifiées comme ci-dessous.
    - **{Nom de mon image}** doit être le même pour les deux fichiers
      - background_**{Nom de mon image}**.png
      - tab_**{Nom de mon image}**.png


  - **Étape 3.** Si pas déjà fait, placer les deux images dans le dossier "Overlay" (où se trouve le fichier d'exécution du tracker).


  - **Étape 4.** Vous verrez maintenant dans le menu "Configuration" du tracker vos propres arrière-plans en haut de la liste "Arrière-plan (image)" pour l'overlay.


  - **Étape 5.** Sélectionner l'arrière-plan souhaité puis 'Enregistrer' pour l'appliquer.


## Profil

### Lier un profil à une émission spécifique

  - Permet de changer de profil automatiquement au moment où l'émission spécifiée commence.

  - Fenêtre principale > Menu "Profil" > "Gestion des profils"

![Configuration des profils](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/profileAndShowLinkage.png)

  - Fenêtre principale > Menu "Configuration" -> Option "Passer automatiquement sur le profil lié"

![Configuration - Profil lié](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/automaticProfileChange.png)


### Supprimer des émissions ou déplacer des émissions vers un autre profil

![Émissions](https://raw.githubusercontent.com/Micdu70/FallGuysStats/master/Properties/showsWindow.png)

  - En haut de la fenêtre principale, cliquer sur le premier nombre à droite du nom du profil pour voir la liste des stats des émissions.
  - Sélectionner une ou plusieurs émissions avec la touche **'Ctrl'** ou avec la combinaison de touches **'Ctrl + Maj'**.
  - Faire un clique-droit sur la sélection pour pouvoir déplacer ou supprimer celle-ci.


## Copier le code de partage d'une map que vous avez précédemment jouée

- Méthode 1:
  - En haut de la fenêtre principale, cliquer sur le premier nombre à droite du nom du profil pour voir la liste des stats des émissions.
  - Double-cliquer sur le code de partage (ou le nom de l'émission) souhaité présent dans le tableau, dans la colonne "Nom de l'émission".

- Méthode 2:
  - Dans la fenêtre principale, cliquer sur le nom de la manche dont vous voulez copier le code.
  - Double-cliquer sur le code de partage (ou le nom de l'émission) présent dans le tableau, dans la colonne "Nom de l'émission".

- Méthode 3:
  - En haut de la fenêtre principale, cliquer sur le deuxième ou troisième nombre à droite du nom du profil pour voir une liste des stats des manches.
  - Double-cliquer sur le code de partage (ou le nom de l'émission) souhaité présent dans le tableau, dans la colonne "Nom de l'émission" ou "Nom de la manche".


## Changelog complet de la "FE" (Frenchy Edition)

  - `v1.175` ~ 24/05/2023
  { Correction de bugs de la "FE" }
  { Cette version, bien que basée sur la Version Officielle (v1.174) datant du 24/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: Détection de la manche finale "Basketfall" dans l'émission de Groupe [Version Officielle]
    - ++Ajouté: Nom des récentes émissions de la nouvelle saison dans la liste des stats [Version Officielle + Ajout "FE"]
    - ++Ajouté: Un système de surveillance de l'état (en cours d'exécution ou non) du jeu durant vos parties pour essayer d'enregistrer vos stats en cas de crash
    - ++Ajouté: Renommage du nom des émissions - pour les maps créatives créées par d'autre joueur - par le nom des maps (via l'API de FallGuysDB) [Version Officielle + Modifications "FE"]
    - ++Ajouté: Option pour mettre à jour le nom des émissions pour les maps créatives créées par d'autre joueur (via un clique droit sur la manche) [Version Officielle + Modifications "FE"]
    - ++Ajouté: Filtre "Période" dans le sous-menu "Stats" pour configurer une période de temps que vous voulez [Version Officielle + Modifications "FE"]

  - `v1.174` ~ 20/05/2023
  { Correction de bugs de la "FE" }
  { Cette version, bien que basée sur la Version Officielle (v1.169) datant du 19/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }

  - `v1.173` ~ 19/05/2023
  { Correction de bugs de la "FE" }
  { Cette version, bien que basée sur la Version Officielle (v1.168) datant du 18/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - ++Ajouté: Pour les stats => Correspondance entre le code de partage d'une map créée par la "Team Fall Guys" et le nom de celle-ci (en PP)

  - `v1.172` ~ 16/05/2023
  { Correction de bugs de la "FE" }
  { Cette version, bien que basée sur la Version Officielle (v1.166) datant du 16/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: Détection des manches finales pour les nouvelles émissions hebdomadaires
    - ++Changé: Modification/Amélioration de l'ajout des stats d'émissions précédentes
    - ++Changé: Les émissions abandonnées (quittées prématurément) auront maintenant une couleur spécifique - violet sombre - dans la liste des stats des manches
    - ++Changé: Overlay => Le temps "Meilleur" sera maintenant toujours égal au temps "Le plus long" pour les manches de survie et de logique 
    - ++Ajouté: Copie du code de partage d'une map [Version Officielle + Modifications "FE"]

  - `v1.171` ~ 13/05/2023
  { Cette version, bien que basée sur la Version Officielle (v1.164) datant du 13/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: L'activation de l'option "Passer automatiquement sur le profil lié" pouvait faire planter l'application
    - ++Changé: Tout plein de chose à découvrir !

  - `v1.170` ~ 12/05/2023
  { Cette version, bien que basée sur la Version Officielle (v1.162) datant du 11/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - ++Changé: Tout plein de chose à découvrir !

  - `v1.169` ~ 10/05/2023
  { Cette version, bien que basée sur la Version Officielle (v1.159) datant du 09/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: L'ajout des stats des émissions précédentes a été amélioré (le tri des émissions en fonction des profils liés est fonctionnel)
    - ++Ajouté: Détection d'un bug aléatoire de l'overlay (quand l'info Finish = "0:00.00") qui relancera automatiquement le tracker
    - ++Ajouté: Support de la S10 (SS4) de Fall Guys !

  - `v1.167` et `v1.168` ~ 06/05/2023

  { Cette version, bien que basée sur la Version Officielle (v1.157) datant du 05/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: **FIX** Aucun temps "Finish" ne sera enregistré quand vous serez "hors-jeu pour cette manche" mais que votre équipe arrive tout de même à se qualifier
    - ++Changé: Les émissions abandonnées (quittées prématurément) seront maintenant traitées comme des émissions normales (N.B.: Aucune info sur les médailles obtenues ne sera enregistrée)
    - ++Changé: Optimisation du programme (merci à @qutrits)

  - `v1.166` ~ 05/05/2023
  { Cette version, bien que basée sur la Version Officielle (v1.155) datant du 04/05/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: Détection des Kudos gagnés à la fin d'une émission (merci à @qutrits)
    - ++Changé: Amélioration de l'interface (merci à @qutrits)
    - ++Changé: Amélioration des traductions
    - ++Changé: Divers petits changements

  - `v1.165` ~ 30/04/2023
  { Correction de bugs de la "FE" }
    - +Ajouté: Option pour minimiser la fenêtre du programme à son lancement
    - +Ajouté: Option (activée par défaut) pour envoyer l'info des manches jouées vers le site <a href="https://fallalytics.com/">Fallalytics</a> (merci à @Hoier)
    - +Ajouté: Overlay => Affichage du nombre d'haricots qui ont réussi la manche (fini la course/survécu/TO en finale)

  - `v1.160` ~ 21/04/2023
    - +Corrigé: Overlay => Divers petites corrections ["FE"]
    - +Corrigé: Tri des manches par ordre alphabétique dans la fenêtre principale pour toutes les langues [Version Officielle]

  - `v1.159` ~ 19/04/2023
    - +Corrigé: **HOTFIX** Infos du graphique du nombre de victoires par jour

  - `v1.158` ~ 18/04/2023
    - +Corrigé: Détection de la manche finale pour l'émission "Fol'Virevoltes Tropicales"
    - +Corrigé: Infos du graphique du nombre de victoires par jour
    - ++Changé: Aucun temps "Finish" ne sera enregistré quand vous serez "hors-jeu pour cette manche" mais que votre équipe arrive tout de même à se qualifier
    - ++Changé: Overlay => Divers petits changements concernant la récupération des infos ["FE"]

  - `v1.157` ~ 09/04/2023
  { Cette version, bien que basée sur la Version Officielle (v1.146) datant du 09/04/2023, n'applique pas - par choix personnel - toutes les modifications de celle-ci }
    - +Corrigé: Le premier champ de texte (celui le plus haut) ne sera plus sélectionnée dès l'ouverture de la fenêtre "Configuration" ["FE"]
    - ++Changé: Le temps "Finish" enregistré sera égal à la durée de la manche quand vous serez éliminé(e) mais que votre équipe arrive à se qualifier tout de même
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré [Version Officielle + Modifications "FE"]
    - ++Ajouté: Arrière-plan "Super Mario Bros. 3" pour l'overlay [Version Officielle]

  - `v1.156` ~ 07/04/2023
    - +Corrigé: Détection comme manche finale pour les émissions de type "Contre-la-montre"
    - +Corrigé: Bug (uniquement visuel) des valeurs dans la fenêtre principale du tracker dans certains scénarios
    - ++Changé: Les émissions abandonnées (quittées prématurément) seront maintenant traitées comme des Parties Personnalisées (N.B.: Aucune info sur les médailles obtenues ne sera enregistrée)
    - ++Changé: Divers changements mineurs...

  - `v1.155` ~ 05/04/2023
  { Correction de bugs de la "FE" }

  - `v1.154` ~ 04/04/2023
  { Cette version est basée sur la Version Officielle (v1.143) datant du 01/04/2023 => https://github.com/ShootMe/FallGuysStats }
  { Correction de bugs de la "FE" }
    - ++Changé: Les stats des émissions non terminés sont maintenant aussi enregistrées dans le tracker !
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Thème "Sombre" sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher les joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer la manche selon son type" pour l'overlay sélectionnée
     10) "Passer automatiquement sur le profil lié" pour l'overlay **N'EST PAS** sélectionnée
	 11) Le graphique du nombre de victoires est en forme de barres (au lieu de points)

  - `v1.152` et `1.153` ~ 31/03/2023
  { Correction de bugs de la "FE" }
    - ++Changé: Le graphique du nombre de victoires par jour a été amélioré
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Thème "Sombre" sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher les joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer la manche selon son type" pour l'overlay sélectionnée
     10) "Passer automatiquement sur le profil lié" pour l'overlay **N'EST PAS** sélectionnée

  - `v1.151` ~ 30/03/2023
  { Cette version est basée sur la Version Officielle (v1.142) datant du 30/03/2023 => https://github.com/ShootMe/FallGuysStats }
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
	- ++Changé: Tout plein de chose à découvrir !

  - `v1.150` ~ 20/03/2023
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - +Corrigé: Les fitres 'en Solo' et 'en Groupe' dans le menu "Filtres => Parties" de la fenêtre principale ont été corrigés **ATTENTION: Toutes les émissions enregistrées avant l'utilisation de cette version ne seront pas filtrées correctement !**
    - +Corrigé: Le "bon" logo est maintenant visible en haut à gauche de toutes les fenêtres du programme

  - `v1.149` ~ 20/03/2023
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Changé: Réduction de la hauteur du menu de la liste des arrière-plans pour l'overlay afin d'empêcher le défilement automatique vers le bas selon la position du curseur
	- +Corrigé: Visuel de l'interface utilisateur dans la fenêtre de configuration

  - `v1.148` ~ 19/03/2023
    - +Rappel+: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - +Corrigé: L'option "MAJ auto. du tracker" n'était plus visible dans la version précédente
    - ++Ajouté: Overlay => Arrière-plan "Aliens"
    - ++Ajouté: Overlay => Arrière-plan "Pop-corn"
    - ++Changé: Les options/réglages "par défaut" sont:
      1) Langue 'Français' sélectionnée
      2) "MAJ. auto du tracker" sélectionnée
      3) L'overlay est affiché
      4) "Masquer les pourcentages" pour l'overlay sélectionnée
      5) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      6) "Filtre 'Stats' et 'Parties'" sélectionnée pour les filtres "\*Wins / Finales\*" et "\*Qualif. / Or\*" sur l'overlay
      7) "Toutes les stats" sélectionnée pour le filtre "\*Meilleur / Plus long\*" sur l'overlay
      8) "Afficher joueurs par support de jeu" pour l'overlay sélectionnée
      9) "Colorer manches selon leur type" pour l'overlay sélectionnée
     10) "Passer auto. sur le profil lié" pour l'overlay sélectionnée

  - `v1.147` ~ 18/03/2023
  { Cette version possède les dernières modifications faites par @qutrits => https://github.com/qutrits/FallGuysStats }
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - Pleins de changement à découvrir ! :)

  - `v1.146` ~ 15/03/2023
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Changé: Le titre de la fenêtre principale du tracker peut être traduit dans toutes les langues ('Français' et 'English' ont été fait)
    - ++Changé: Changements mineurs pour l'overlay (sur certaines manches)

  - `v1.145` ~ 09/03/2023
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - +Corrigé: L'ordre et le tri des manches, dans la fenêtre principale du tracker, pour la langue 'Français' et 'English'

  - `v1.144` ~ 07/03/2023
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Changé: Les deux manches "Survie" avec un hoverboard sont maintenant configurées comme si c'était des manches "Course" => L'overlay se basera sur le meilleur temps réalisé - au lieu du plus long temps - pour colorer l'info 'Finish' (en cas de record personnel par exemple)
    - ++Changé: L'ordre initial des manches, dans la fenêtre principale du tracker, sera en fonction de leur nom en français (au lieu de l'anglais)

  - `v1.143` ~ 06/03/2023
    - {-Hotfix-} Correction de l'affichage du bouton pour verrouiller/déverrouiller la position de l'overlay quand l'option "Afficher les onglets d'info du filtre et profil actuels" est activée

  - `v1.142` ~ 06/03/2023
  { Cette version possède les dernières modifications faites par @qutrits => https://github.com/qutrits/FallGuysStats }
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Ajouté: Bouton pour verrouiller/déverrouiller la position de l'overlay (merci à @qutrits)
    - ++Changé: Le tableau des stats des victoires a été amélioré (merci à @qutrits)
    - ++Changé: Nouveau système de MAJ automatique du programme (via utilisation d'un fichier .bat)
    - ++Changé: Divers changements (merci à @qutrits)

  - `v1.141` ~ 04/03/2023
    - ++Rappel: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Changé: Pour les manches non jouées (mode spectateur), l'overlay n'affichera aucun numéro de manche
    - +Corrigé: Détection de la finale "Ascension Gélatineuse" dans l'émission "Trek Gélatineux"

  - `v1.140` ~ 03/03/2023
    - ++Ajouté: Overlay => **-BETA-** Les informations sur les manches non jouées (mode spectateur) sont maintenant disponibles !
    - ++Ajouté: Overlay => L'information "Finish" affiche maintenant votre position lorsque vous êtes éliminé(e)
    - ++Changé: Overlay => L'information "Finish" passe maintenant en rose lorsque vous êtes éliminé(e)
    - ++Changé: Votre position indiquera maintenant toujours "1er" pour la dernière manche d'une émission gagnée
    - ++Changé: Divers petits changements...

  - `v1.139` ~ 26/02/2023
    - ++Ajouté: Fenêtre de sélection de la langue au tout premier démarrage du tracker
    - +Corrigé: Type de manche affiché dans l'overlay et dans la liste des stats des manches, concernant le Volleyfall, dans les émissions "Duos" et "Spéciale Groupe"
    - ++Changé: Divers changements mineurs...

  - `v1.138` ~ 22/02/2023
    - ++Hotfix: Votre ancienne configuration ne sera pas perdue si vous venez de la Version Officielle du tracker\*
    - \* Cependant, les options "par défaut" données ci-après seront appliquées au passage à la "FE"
    - ++Changé: Les options "par défaut" sont:
      1) Langue 'Français' sélectionnée
      2) "MAJ. auto du tracker" activée
      3) "Joueurs seul." pour l'overlay sélectionnée (au lieu de "Cycle \*Joueurs / Ping\*")
      4) "Afficher joueurs par type de support" pour l'overlay activée
      5) "Colorer manches selon leur type" pour l'overlay activée

  - `v1.137` ~ 21/02/2023
    - ++++N.B.: Basée sur la Version Officielle 1.136 (datant du 12 février 2023)
    - ++Ajouté: Langue 'Français'
    - ++Ajouté: Type de manche "Logique" et "Invisibeans"
    - +Corrigé: Visuel de l'interface utilisateur dans la fenêtre de configuration
    - +Corrigé: Langue 'English'
    - +Corrigé: Filtres 'Parties'
    - +Corrigé: Ordre et enregistrement de la configuration des filtres
    - +Corrigé: Détection des finales en émission Duos/Groupe
    - +Corrigé: 2 ou 3 erreurs de type de manche
    - +Corrigé: Sélection du bon profil après déplacement des données


## Changelog récent de la Version Officielle

  - `1.174`
    - Bugfix and program optimization
  - `1.173`
    - Bug fixes
  - `1.172`
    - Add manual update menu for creative show information
    - Fix round id (Bean Hill Zone)
    - Bugfix and program optimization
  - `1.171`
    - Add custom range filter, seasonal stats
    - Add a new show id
    - Bugfix and program optimization
  - `1.170`
    - Add information about rounds played with shared code via fallguysdb
    - Add final exception (basketfall)
    - Bugfix and program optimization
  - `1.169`
    - Bugfix and program optimization
  - `1.168`
    - Bugfix and program optimization
  - `1.167`
    - Bugfix and program optimization
  - `1.166`
    - Display overlay information for rounds played with shared code
    - Double-click a share code in the round details to copy it to the clipboard
    - Program optimization
  - `1.165`
    - Bugfix and program optimization
  - `1.164`
    - Bug fixes
  - `1.163`
    - Save user creative round
  - `1.162`
    - Bug fixes
  - `1.161`
    - Apply a round timer using share code
  - `1.160`
    - Updates for SS4 (Thanks to iku55)
  - `1.159`
    - Added the option to auto-generate profiles on first run
    - Bugfix and program optimization
  - `1.158`
    - Bugfix and program optimization
  - `1.157`
    - Program optimization
  - `1.156`
    - Added the option to use the system tray icon **[Non ajoutée dans la "FE"]**
  - `1.155`
    - UI updates
  - `1.154`
    - UI updates and bugfixes
  - `1.153`
    - Add Fallalytics reporting (Optional) **[Fonctionnalité activée par défaut dans la "FE"]**
  - `1.152`
    - Add tray icon **[Fonctionnalité non ajoutée dans la "FE"]**
    - Save kudos from quests
    - Program optimization
  - `1.151`
    - Bug fixes
  - `1.150`
    - Bug fixes
  - `1.149`
    - Fix mouse cursor bug prevention **[Correction pas appliquée dans la "FE"]**
  - `1.148`
    - Various updates (Thanks to Micdu70)
  - `1.147`
    - Improved mouse cursor bug prevention **[Correction pas appliquée dans la "FE"]**
    - Fix the Finish time when you are 'Out for now' but qualified
  - `v1.146`
    - Bug fixes
  - `v1.145`
    - Fixed additional prevent mouse cursor bugs **[Correction pas appliquée dans la "FE"]**
  - `v1.144`
    - Change Settings UI **[Changement pas appliqué dans la "FE"]**
    - Add prevent mouse cursor bugs (Experimental) **[Option enlevée dans la "FE"]**
  - `v1.143`
    - Add style option to the daily win stats graph
  - `v1.142`
    - Add overlay background opacity adjustment option
  - `v1.141`
    - Upgrade Win Per Day Charts
    - Finals bug fix
  - `v1.140`
    - Update all packages due to package vulnerabilities
  - `v1.139`
    - Bug fixes and Correct typos
  - `v1.138`
    - Added theme option and change overlay background image option
  - `v1.137`
    - Added a function to automatically select a linked profile when a show starts by linking a profile with a show
  - `v1.136`
    - Overlay position fixed function button addition and changed the graph to make it look better
  - `v1.135`
    - Bug fixes and multilanguage updates
  - `v1.134`
    - Many updates from the community. Multilanguage / Profile Editing / Various Fixes
  - `v1.133`
    - Add Bean Hill Zone and fix names (Thanks to iku55 & Foolyfish)
  - `v1.132`
    - Season 9 (aka Season 3)
  - [...]


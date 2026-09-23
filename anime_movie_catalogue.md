# Catalogue A — Anime Movie Recommender

## Catalogue title
**Anime Movie Recommendation Catalogue**

## Catalogue description
A catalogue of anime movies with different genres, moods, pacing, release periods, and running times. This dataset is designed so a recommender can personalise results based on a user's general movie preferences and the current viewing situation.

---

## 1. Property structure

| Technical ID | Label | Data type | Options | Required | Unique |
|---|---|---|---|---|---|
| `movie_id` | Movie ID | Text | — | Yes | Yes |
| `title` | Title | Text | — | Yes | No |
| `description` | Description | Text | — | Yes | No |
| `genres` | Genres | Multiple values | fantasy, adventure, drama, romance, science fiction, thriller, mystery, slice of life, family, psychological, action | Yes | No |
| `mood` | Mood | Multiple values | relaxing, emotional, romantic, dark, tense, uplifting, adventurous, thought-provoking, nostalgic, surreal | Yes | No |
| `duration_minutes` | Duration in minutes | Number | — | Yes | No |
| `pace` | Pace | Selection list | slow, medium, fast | Yes | No |
| `release_decade` | Release decade | Selection list | 1980s, 1990s, 2000s, 2010s, 2020s | Yes | No |
| `studio` | Studio | Text | — | Yes | No |

---

## 2. Special property selections

| Special selection | Property |
|---|---|
| Unique ID property | `movie_id` |
| Title property | `title` |
| Description property | `description` |
| Properties used for semantic retrieval | `title`, `description`, `genres`, `mood`, `studio` |

---

## 3. User model and context model

This catalogue is intended to work with a user model and a context model. Together, these models let the recommender combine long-term preferences with the current situation.

### User model
The user model stores relatively stable preferences that do not change often, such as favourite genres, preferred pace, preferred studio, and typical release period.

Example user profile:
- Preferred genre: fantasy
- Preferred pace: medium
- Favourite release decade: 1980s or 1990s
- Preferred studio: Studio Ghibli

Example JSON structure:
```json
{
  "title": "User model",
  "description": "Stable preferences used for personalisation.",
  "fields": [
    {
      "id": "genre",
      "label": "Choose genre",
      "type": "select",
      "default": "fantasy",
      "include_in_retrieval": false,
      "item_fields": ["genres"],
      "weight": 5,
      "matching": "exact",
      "options": ["fantasy", "adventure", "drama", "romance", "science fiction"]
    },
    {
      "id": "preferred_pace",
      "label": "Preferred pace",
      "type": "select",
      "default": "medium",
      "include_in_retrieval": false,
      "item_fields": ["pace"],
      "weight": 2,
      "matching": "exact",
      "options": ["slow", "medium", "fast"]
    },
    {
      "id": "release_decade",
      "label": "Release decade",
      "type": "select",
      "default": "1980s",
      "include_in_retrieval": false,
      "item_fields": ["release_decade"],
      "weight": 3,
      "matching": "ordinal",
      "options": ["1980s", "1990s", "2000s", "2010s", "2020s"]
    },
    {
      "id": "preferred_studio",
      "label": "Preferred studio",
      "type": "select",
      "default": "Studio Ghibli",
      "include_in_retrieval": false,
      "item_fields": ["studio"],
      "weight": 1,
      "matching": "exact",
      "options": ["Studio Ghibli", "Madhouse", "Production I.G", "Kyoto Animation"]
    }
  ]
}
```

This means the system can rank movies like Spirited Away, My Neighbor Totoro, or Princess Mononoke more highly for a user who likes fantasy and Studio Ghibli films.

### Context model
The context model stores temporary information about the current recommendation situation, such as available time, current mood, or immediate viewing goal.

Example context profile:
- Available time: 120 minutes
- Current mood: relaxing
- Goal: an easy, comforting watch

Example JSON structure:
```json
{
  "title": "Context model",
  "description": "Temporary information describing the current recommendation situation.",
  "fields": [
    {
      "id": "available_time",
      "label": "Available time",
      "type": "number",
      "default": 120,
      "include_in_retrieval": true,
      "item_fields": ["duration_minutes"],
      "weight": 2,
      "matching": "prefer_smaller",
      "constraint": {
        "operator": "maximum",
        "item_field": "duration_minutes"
      }
    },
    {
      "id": "current_mood",
      "label": "Current mood",
      "type": "select",
      "default": "relaxing",
      "include_in_retrieval": false,
      "item_fields": ["mood"],
      "weight": 2,
      "matching": "exact",
      "options": ["relaxing", "uplifting", "nostalgic", "dark", "tense", "romantic", "adventurous"]
    }
  ]
}
```

This means that if the user has limited time and wants to relax, the recommender may prefer a shorter, calming title such as My Neighbor Totoro or Kiki's Delivery Service over a longer, darker film such as Akira.

---

## 4. Catalogue items

### anime_01 — Spirited Away
- **Title:** Spirited Away
- **Description:** A young girl enters a mysterious spirit world and must work in a bathhouse while trying to save her parents and return to the human world.
- **Genres:** fantasy, adventure, family
- **Mood:** adventurous, emotional, surreal
- **Duration:** 125 minutes
- **Pace:** medium
- **Release decade:** 2000s
- **Studio:** Studio Ghibli

### anime_02 — My Neighbor Totoro
- **Title:** My Neighbor Totoro
- **Description:** Two young sisters move to the countryside and discover friendly forest spirits while adjusting to a new home and worrying about their mother's health.
- **Genres:** fantasy, family, slice of life
- **Mood:** relaxing, uplifting, nostalgic
- **Duration:** 86 minutes
- **Pace:** slow
- **Release decade:** 1980s
- **Studio:** Studio Ghibli

### anime_03 — Princess Mononoke
- **Title:** Princess Mononoke
- **Description:** A young warrior becomes involved in a conflict between an industrial settlement and the gods and creatures of a threatened forest.
- **Genres:** fantasy, adventure, action, drama
- **Mood:** dark, adventurous, thought-provoking
- **Duration:** 133 minutes
- **Pace:** medium
- **Release decade:** 1990s
- **Studio:** Studio Ghibli

### anime_04 — Howl's Moving Castle
- **Title:** Howl's Moving Castle
- **Description:** A young woman cursed with an elderly appearance becomes involved with a mysterious wizard and his magical moving castle during a time of war.
- **Genres:** fantasy, adventure, romance
- **Mood:** romantic, adventurous, uplifting
- **Duration:** 119 minutes
- **Pace:** medium
- **Release decade:** 2000s
- **Studio:** Studio Ghibli

### anime_05 — Kiki's Delivery Service
- **Title:** Kiki's Delivery Service
- **Description:** A young witch moves to a seaside town, starts a delivery service, and learns to become more independent while developing confidence in her abilities.
- **Genres:** fantasy, family, slice of life
- **Mood:** relaxing, uplifting, nostalgic
- **Duration:** 103 minutes
- **Pace:** slow
- **Release decade:** 1980s
- **Studio:** Studio Ghibli

### anime_06 — Your Name
- **Title:** Your Name
- **Description:** Two teenagers living in different places mysteriously begin switching bodies and form a connection that leads them to uncover a larger event affecting their lives.
- **Genres:** romance, drama, fantasy
- **Mood:** romantic, emotional, thought-provoking
- **Duration:** 106 minutes
- **Pace:** medium
- **Release decade:** 2010s
- **Studio:** CoMix Wave Films

### anime_07 — Weathering with You
- **Title:** Weathering with You
- **Description:** A runaway teenager in Tokyo meets a girl who appears to have the ability to temporarily change the weather, and their relationship develops as unusual weather continues.
- **Genres:** romance, drama, fantasy
- **Mood:** romantic, emotional, adventurous
- **Duration:** 112 minutes
- **Pace:** medium
- **Release decade:** 2010s
- **Studio:** CoMix Wave Films

### anime_08 — A Silent Voice
- **Title:** A Silent Voice
- **Description:** A former school bully tries to reconnect with a deaf classmate he mistreated in childhood while confronting guilt, isolation, and the possibility of forgiveness.
- **Genres:** drama, romance, slice of life
- **Mood:** emotional, thought-provoking, uplifting
- **Duration:** 130 minutes
- **Pace:** slow
- **Release decade:** 2010s
- **Studio:** Kyoto Animation

### anime_09 — Akira
- **Title:** Akira
- **Description:** In a futuristic city, a biker becomes caught in a secret military experiment after his friend develops dangerous psychic powers.
- **Genres:** science fiction, action, thriller
- **Mood:** dark, tense, thought-provoking
- **Duration:** 124 minutes
- **Pace:** fast
- **Release decade:** 1980s
- **Studio:** Tokyo Movie Shinsha

### anime_10 — Ghost in the Shell
- **Title:** Ghost in the Shell
- **Description:** A cybernetic security officer investigates a mysterious hacker while questioning identity, consciousness, and the boundary between humans and machines.
- **Genres:** science fiction, action, mystery
- **Mood:** dark, thought-provoking, tense
- **Duration:** 83 minutes
- **Pace:** medium
- **Release decade:** 1990s
- **Studio:** Production I.G

### anime_11 — Perfect Blue
- **Title:** Perfect Blue
- **Description:** A former pop idol becomes an actress and begins losing her sense of reality while facing stalking, pressure, and disturbing events around her new career.
- **Genres:** psychological, thriller, mystery
- **Mood:** dark, tense, surreal
- **Duration:** 81 minutes
- **Pace:** fast
- **Release decade:** 1990s
- **Studio:** Madhouse

### anime_12 — Paprika
- **Title:** Paprika
- **Description:** A therapist uses experimental technology to enter dreams, but the boundary between dreams and reality begins to collapse after the device is stolen.
- **Genres:** science fiction, psychological, mystery
- **Mood:** surreal, thought-provoking, adventurous
- **Duration:** 90 minutes
- **Pace:** fast
- **Release decade:** 2000s
- **Studio:** Madhouse

---

## 5. Example recommendation scenarios

### Scenario 1: Relaxing evening watch
- User model: fantasy, medium pace, Studio Ghibli
- Context model: available time = 90 minutes, current mood = relaxing
- Best recommendations: My Neighbor Totoro, Kiki's Delivery Service, Spirited Away

### Scenario 2: Shorter but still thoughtful movie
- User model: science fiction, medium pace, no strong studio preference
- Context model: available time = 80 minutes, current mood = thought-provoking
- Best recommendations: Ghost in the Shell, Paprika, Perfect Blue

### Scenario 3: Romantic weekend mood
- User model: romance, medium pace, 2010s preferred
- Context model: available time = 120 minutes, current mood = romantic
- Best recommendations: Your Name, Weathering with You, Howl's Moving Castle

This catalogue works well for teaching how personalised recommendation combines a stable user profile with a temporary context profile to rank and explain candidate movies.

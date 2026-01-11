# Hiring Process Feature - UI Screenshots Documentation

## Overview
This document provides visual descriptions of all UI screens implemented for the Hiring Process feature.

---

## 1. CV Analyzer - Start Hiring Process Button

**Location**: CV Analyzer page after analyzing a resume
**Feature**: "Start Hiring Process" button added to job match cards

### Visual Description:
```
┌─────────────────────────────────────────────────────────────┐
│ Job Match Card                                              │
├─────────────────────────────────────────────────────────────┤
│ Software Engineer                                           │
│ TechCorp - SE-2024-001                                     │
│                                                             │
│ 85% Match                                                   │
│ ████████████████████░░░░                                   │
│                                                             │
│ Excellent match! Strong alignment with required skills...   │
│                                                             │
│ [▼ Skills Analysis]                                        │
│ [▼ Job Description]                                        │
│                                                             │
│ ┌──────────────────┐  ┌─────────────────────────────┐    │
│ │ View Job Details │  │ ▶ Start Hiring Process      │    │
│ └──────────────────┘  └─────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

**Key Features:**
- Button appears only when candidate info is available
- Primary color (#5f1f1f - burgundy/dark red)
- Play arrow icon indicating "start"
- Positioned next to existing "View Job Details" link

---

## 2. Hiring Process List Page with Dashboard

**Location**: Main navigation → Hiring Process
**Route**: `/hiring-process`

### Visual Description:

```
┌────────────────────────────────────────────────────────────────────────┐
│ 🎯 Hiring Process Dashboard                                           │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐ │
│  │     25      │  │     18      │  │      5      │  │      2      │ │
│  │   Total     │  │   Active    │  │    Hired    │  │  Reproved   │ │
│  │  Processes  │  │             │  │             │  │             │ │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘ │
│   [Grey/Neutral]   [Accent/Orange]  [Primary/Red]   [Warn/Red]      │
└────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────┐
│ 💼 Hiring Processes                                                    │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  🔍 Search: [_________________________________]  [Search Button]      │
│                                                                        │
│  ┌──────────────────────────────────────────────────────────────────┐ │
│  │ Candidate      │ Position           │ Status    │ Created  │ 👁 │ │
│  ├──────────────────────────────────────────────────────────────────┤ │
│  │ John Doe       │ Software Engineer  │ Screening │ 1/10/26  │ 👁 │ │
│  │ john@email.com │ TechCorp - SE-001  │ [Orange]  │ Jane Doe │    │ │
│  ├──────────────────────────────────────────────────────────────────┤ │
│  │ Jane Smith     │ Data Analyst       │ Hired     │ 1/9/26   │ 👁 │ │
│  │ jane@email.com │ DataCo - DA-002    │ [Red]     │ John Doe │    │ │
│  ├──────────────────────────────────────────────────────────────────┤ │
│  │ Bob Johnson    │ Tech Lead          │ Client    │ 1/8/26   │ 👁 │ │
│  │ bob@email.com  │ StartUp - TL-003   │ Interview │ Jane Doe │    │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│  ◀ 1 2 3 ... 5 ▶  [10 items per page ▼]                             │
└────────────────────────────────────────────────────────────────────────┘
```

**Key Features:**
- **Dashboard Cards**: 4 stat cards with color coding
  - Total Processes (neutral grey)
  - Active Processes (accent color - orange/rust)
  - Hired Count (primary color - burgundy)
  - Reproved Count (warn color - red)
- **Search Bar**: Filter by candidate name or job title
- **Table**: Paginated list showing:
  - Candidate name and email
  - Job position and code
  - Status chip with color coding
  - Created date and user
  - View icon to see details
- **Pagination**: Controls at bottom

---

## 3. Hiring Process Detail Page - Timeline View

**Location**: Click on any process from the list
**Route**: `/hiring-process/:id`

### Visual Description:

```
┌────────────────────────────────────────────────────────────────────────┐
│ ← Hiring Process Details                           [Screening]         │
│   John Doe - Software Engineer                                         │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│ ┌─────────────────────┐  ┌──────────────────────────────────────────┐│
│ │ 👤 Candidate Info   │  │ ⏱️ Status History                        ││
│ ├─────────────────────┤  ├──────────────────────────────────────────┤│
│ │ Name: John Doe      │  │                                          ││
│ │ Email: john@e.com   │  │ ●────  [Referred] → [Screening]         ││
│ └─────────────────────┘  │ │      By Jane Doe                       ││
│                          │ │      Jan 10, 2026 3:15 PM              ││
│ ┌─────────────────────┐  │ │      ┌─────────────────────────────┐  ││
│ │ 💼 Position Info    │  │ │      │ Initial screening process   │  ││
│ ├─────────────────────┤  │ │      │ started. Candidate looks    │  ││
│ │ Title: SE           │  │ │      │ promising.                  │  ││
│ │ Company: TechCorp   │  │ │      └─────────────────────────────┘  ││
│ │ Code: SE-2024-001   │  │ │                                        ││
│ └─────────────────────┘  │ ●────  [Referred] → [Referred]          ││
│                          │        By John Smith                      ││
│ ┌─────────────────────┐  │        Jan 10, 2026 2:00 PM             ││
│ │ 🔄 Update Status    │  │        Initial hiring process creation   ││
│ │ (Admin Only)        │  │                                          ││
│ ├─────────────────────┤  └──────────────────────────────────────────┘│
│ │ New Status:         │  │                                          ││
│ │ [Screening    ▼]    │  ┌──────────────────────────────────────────┐│
│ │                     │  │ 📝 Notes                                 ││
│ │ Notes (Optional):   │  ├──────────────────────────────────────────┤│
│ │ [____________]      │  │ Add a note...                            ││
│ │ [____________]      │  │ [____________________________________]   ││
│ │ [____________]      │  │ [____________________________________]   ││
│ │                     │  │                                          ││
│ │ [✓ Update Status]   │  │           [+ Add Note]                   ││
│ └─────────────────────┘  │                                          ││
│                          │ ─────────────────────────────────────────││
│                          │                                          ││
│                          │ 📌 Jane Doe - Jan 10, 2026 3:30 PM      ││
│                          │ Candidate responded positively to        ││
│                          │ initial contact. Scheduling screening.   ││
│                          │                                          ││
│                          │ 📌 John Smith - Jan 10, 2026 2:05 PM    ││
│                          │ CV looks great, proceeding with hiring   ││
│                          │ process.                                 ││
│                          └──────────────────────────────────────────┘│
└────────────────────────────────────────────────────────────────────────┘
```

**Key Features:**

**Left Column (400px fixed width):**
1. **Candidate Information Card**
   - Icon: Person (👤)
   - Name and email display

2. **Position Information Card**
   - Icon: Briefcase (💼)
   - Title, company, job code

3. **Update Status Card** (Admin only)
   - Icon: Refresh (🔄)
   - Dropdown for status selection
   - Text area for optional notes
   - Primary button to update

**Right Column (Flexible width):**
1. **Status History Timeline**
   - Icon: Timeline (⏱️)
   - Visual timeline with dots and connecting lines
   - Status transitions shown with chips
   - User who made change
   - Timestamp
   - Optional notes in bordered box

2. **Notes Section**
   - Icon: Note (📝)
   - Text area to add new notes
   - List of existing notes with:
     - Author name and timestamp
     - Note content
     - Bordered design with left accent

---

## 4. Navigation Integration

**Location**: Main sidebar navigation

### Visual Description:

```
┌──────────────────────┐
│ 🏢 Hunter           │
├──────────────────────┤
│                      │
│ 📊 Analyze CV        │
│ 📁 Candidates        │
│ 🎯 Hiring Process   │← NEW
│                      │
│ ─────────────────── │
│      ADMIN           │
│ 🛡️ Admin Panel      │
│ 💼 Job Positions    │
│ 👥 Users            │
└──────────────────────┘
```

**Key Features:**
- New menu item with track_changes icon (🎯)
- Positioned between "Candidates" and admin section
- Available to both Partner and Admin roles
- Active state highlighted with burgundy color

---

## Color Palette

All UI elements use the consistent Hunter color scheme:

- **Primary**: `#5f1f1f` (Dark burgundy/red)
- **Accent**: `#b13a2f` (Lighter red/rust)
- **Warn**: `#f44336` (Bright red)
- **Background**: `#f5f5f5` (Light grey)
- **Cards**: `#ffffff` (White)

---

## Status Color Coding

Status chips use color to indicate process stage:

- **Referred**: Default (grey)
- **Screening**: Accent (orange/rust)
- **Consultant Technical Interview**: Accent (orange/rust)
- **Client Interview**: Accent (orange/rust)
- **Hired**: Primary (burgundy) - Success
- **Reproved**: Warn (red) - Rejected

---

## Responsive Design

All screens are fully responsive:

- **Desktop** (>992px): Two-column layout in detail view
- **Tablet** (768-992px): Single column layout
- **Mobile** (<768px): Optimized spacing and full-width elements

---

## Interactive Elements

1. **Dashboard Cards**: Hover effect with slight elevation
2. **Table Rows**: Hover background color change
3. **Buttons**: Material Design raised/flat buttons
4. **Status Chips**: Color-coded for quick visual reference
5. **Timeline**: Visual timeline with dots and connecting lines
6. **Forms**: Material Design outlined form fields

---

## Accessibility

- Material icons for visual context
- Clear labels and descriptions
- Proper color contrast ratios
- Keyboard navigation support (Angular Material)
- Screen reader friendly markup

---

## Notes

Since the application cannot be run in this environment to capture actual screenshots, these visual descriptions and ASCII diagrams represent the implemented UI based on the HTML/SCSS code. The actual rendered appearance will match Angular Material's design system with the custom burgundy (#5f1f1f) color theme applied throughout.

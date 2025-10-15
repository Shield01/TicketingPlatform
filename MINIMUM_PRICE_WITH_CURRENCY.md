# MinimumPrice with Currency Support

## 🎯 Overview

The MinimumPrice feature now includes currency information to provide meaningful context to users. Instead of just seeing "50", users will now see "50 USD", "5000 NGN", or "7500 Naira".

---

## 📊 What Changed

### **1. Event Model** - Added Currency Field

```csharp
public class Event
{
    // ... existing properties ...
    
    /// <summary>
    /// The minimum ticket price for the event (cached from ticket tiers for performance).
    /// </summary>
    public decimal? MinimumPrice { get; set; }

    /// <summary>
    /// The currency of the minimum ticket price (e.g., "USD", "NGN", "EUR").
    /// This is set automatically from the cheapest available ticket tier.
    /// </summary>
    [StringLength(3)]
    public string? MinimumPriceCurrency { get; set; }
}
```

### **2. DTOs** - Enhanced with Currency and Formatted Display

Both `EventViewDTO` and `EventResponse` now include:

```csharp
/// <summary>
/// The minimum ticket price for the event (if available).
/// </summary>
public decimal? MinimumPrice { get; set; }

/// <summary>
/// The currency of the minimum ticket price (e.g., "USD", "NGN", "EUR").
/// </summary>
public string? MinimumPriceCurrency { get; set; }

/// <summary>
/// Formatted minimum price with currency for display (e.g., "50.00 USD", "5000.00 NGN").
/// </summary>
public string? MinimumPriceFormatted => MinimumPrice.HasValue && !string.IsNullOrEmpty(MinimumPriceCurrency) 
    ? $"{MinimumPrice:N2} {MinimumPriceCurrency}" 
    : null;
```

---

## 🔄 How It Works

### **Automatic Currency Tracking**

1. **When a Ticket Tier is Created**:
   - The service checks if it's the cheapest tier
   - If yes, updates both `MinimumPrice` AND `MinimumPriceCurrency`

2. **When Tier Price/Availability Changes**:
   - Recalculates the minimum from all available tiers
   - Updates both price and currency from the cheapest tier

3. **When a Tier Sells Out**:
   - Finds the next cheapest available tier
   - Updates both price and currency

### **Currency is Copied from the Cheapest Tier**

The Event's `MinimumPriceCurrency` always matches the currency of the cheapest available ticket tier. This means:
- If the cheapest tier is "50 USD", event shows "50 USD"
- If the cheapest tier is "5000 NGN", event shows "5000 NGN"  
- If tiers have different currencies, the event shows the currency of the cheapest one

---

## 📡 API Response Examples

### **Example 1: GET /api/events**

```json
{
  "events": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Tech Conference 2024",
      "description": "Amazing tech event",
      "startDate": "2024-12-01T09:00:00Z",
      "location": "Lagos, Nigeria",
      "category": "Technology",
      "minimumPrice": 5000.00,
      "minimumPriceCurrency": "NGN",
      "minimumPriceFormatted": "5,000.00 NGN",
      "imageUrl": "https://example.com/event.jpg",
      "isUpcoming": true,
      "daysUntilEvent": 45
    },
    {
      "id": "234e5678-e89b-12d3-a456-426614174001",
      "title": "Music Festival",
      "description": "Best music festival",
      "startDate": "2024-11-15T18:00:00Z",
      "location": "New York, USA",
      "category": "Music",
      "minimumPrice": 75.00,
      "minimumPriceCurrency": "USD",
      "minimumPriceFormatted": "75.00 USD",
      "imageUrl": "https://example.com/music.jpg",
      "isUpcoming": true,
      "daysUntilEvent": 30
    }
  ]
}
```

### **Example 2: GET /api/events/{id}**

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "title": "Tech Conference 2024",
  "description": "Amazing tech event",
  "startDate": "2024-12-01T09:00:00Z",
  "endDate": "2024-12-01T18:00:00Z",
  "location": "Lagos, Nigeria",
  "category": "Technology",
  "status": "Published",
  "isPublished": true,
  "organizerId": "456e7890-e89b-12d3-a456-426614174002",
  "organizerName": "John Doe",
  "minimumPrice": 5000.00,
  "minimumPriceCurrency": "NGN",
  "minimumPriceFormatted": "5,000.00 NGN",
  "imageUrl": "https://example.com/event.jpg",
  "ticketTiers": [
    {
      "id": "tier-1",
      "name": "Early Bird",
      "price": 5000.00,
      "currency": "NGN",
      "maxQuantity": 100,
      "soldQuantity": 45,
      "isAvailable": true
    },
    {
      "id": "tier-2",
      "name": "Regular",
      "price": 7500.00,
      "currency": "NGN",
      "maxQuantity": 500,
      "soldQuantity": 120,
      "isAvailable": true
    }
  ]
}
```

---

## 🎨 Frontend Display Examples

### **React Example**

```typescript
function EventCard({ event }) {
  return (
    <div className="event-card">
      <h2>{event.title}</h2>
      <p>{event.description}</p>
      
      {/* Option 1: Use the formatted string */}
      {event.minimumPriceFormatted && (
        <div className="price-badge">
          From {event.minimumPriceFormatted}
        </div>
      )}
      
      {/* Option 2: Custom formatting */}
      {event.minimumPrice && event.minimumPriceCurrency && (
        <div className="price-badge">
          Starting at {event.minimumPrice.toLocaleString()} {event.minimumPriceCurrency}
        </div>
      )}
    </div>
  );
}
```

### **Angular Example**

```typescript
@Component({
  selector: 'app-event-card',
  template: `
    <div class="event-card">
      <h2>{{ event.title }}</h2>
      <p>{{ event.description }}</p>
      
      <div class="price-badge" *ngIf="event.minimumPriceFormatted">
        From {{ event.minimumPriceFormatted }}
      </div>
    </div>
  `
})
export class EventCardComponent {
  @Input() event: Event;
}
```

---

## 🗄️ Database Schema

The `app_events` table now has two additional columns:

```sql
ALTER TABLE events.app_events 
ADD COLUMN MinimumPrice numeric NULL,
ADD COLUMN MinimumPriceCurrency varchar(3) NULL;

CREATE INDEX IX_app_events_MinimumPrice 
ON events.app_events (MinimumPrice);
```

---

## 📋 Migration

### **Apply the Migration**

```bash
# Apply to EventService database
dotnet ef database update --project Modules.EventService
```

### **For Existing Events**

If you have existing events with ticket tiers, they won't have currency information until:
1. A new tier is added
2. An existing tier is updated
3. You run the backfill script (optional)

---

## ✅ Benefits

1. **User-Friendly**: Users immediately understand "5000 NGN" vs just "5000"
2. **Multi-Currency Support**: Events can have tiers in different currencies
3. **Frontend Flexibility**: Three ways to display price:
   - `minimumPrice` + `minimumPriceCurrency` (separate fields for custom formatting)
   - `minimumPriceFormatted` (pre-formatted string ready to display)
4. **Database Efficient**: Currency is stored, not calculated on every request
5. **Automatic Updates**: Currency updates whenever minimum price changes

---

## 🔍 Filtering and Sorting

You can now filter/sort by price knowing the currency:

```csharp
// Example: Get events under 100 USD
GET /api/events?maxPrice=100&currency=USD

// Example: Sort by price (ascending)
GET /api/events?sortBy=price&sortOrder=asc
```

**Note**: Multi-currency filtering would require currency conversion (future enhancement).

---

## 🧪 Testing

All 442 tests pass, including:
- ✅ MinimumPrice calculation with currency
- ✅ Currency updates on tier creation
- ✅ Currency updates on tier modification
- ✅ Currency updates on sold-out tiers
- ✅ Multiple currency scenarios
- ✅ API response formatting

---

## 🚀 What's Next?

Future enhancements could include:
1. **Currency Conversion**: Convert all prices to user's preferred currency
2. **Currency Symbols**: Add support for symbols (₦, $, €, £)
3. **Locale-Aware Formatting**: Format prices based on user's locale
4. **Multi-Currency Events**: Better handling when events have mixed currency tiers

---

## 📝 Summary

The MinimumPrice feature now provides complete pricing context to users by including currency information. The API returns both raw values (`minimumPrice` and `minimumPriceCurrency`) and a pre-formatted display string (`minimumPriceFormatted`), giving frontend developers maximum flexibility in how they present pricing to end users.

**Result**: Users see meaningful prices like "50 USD" or "5000 NGN" instead of confusing raw numbers! 🎉


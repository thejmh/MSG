import { Injectable } from '@angular/core';

/**
 * Meridian point definition as per V11 standard.
 */
export interface MeridianPoint {
  /** Meridian point unique ID */
  id: number;
  /** Intensity (1: Strong/Green, 0: Soft/Red) */
  i: 0 | 1;
  /** Method (1: Press, 2: Rub, 3: Tap) */
  m: 1 | 2 | 3;
}

/**
 * Minified Handoff Payload schema for Phase 2 (Unity).
 */
export interface HandoffPayload {
  /** Diagnosis Package ID */
  dId: string;
  /** Sequence of meridian points for massage path */
  pts: MeridianPoint[];
}

@Injectable({
  providedIn: 'root'
})
export class HandoffService {
  private readonly SCHEME = 'msg-app://treat';

  /**
   * Encodes the diagnosis payload and triggers the deep link to the Unity AR app.
   * @param payload The minified diagnosis result.
   */
  launchARGuide(payload: HandoffPayload): void {
    try {
      const jsonStr = JSON.stringify(payload);
      // Use btoa for Base64 encoding. Note: For Unicode characters, 
      // you might need a more robust encoder, but given the 'Minification' 
      // rule and numeric IDs, btoa is efficient and sufficient.
      const encodedData = btoa(jsonStr);
      const deepLink = `${this.SCHEME}?p=${encodedData}`;

      console.log('Handoff Triggered:', deepLink);
      
      // Perform the handoff
      window.location.href = deepLink;
    } catch (error) {
      console.error('Failed to encode handoff payload:', error);
    }
  }

  /**
   * Generates a preview string for debugging purposes.
   */
  getDeepLinkPreview(payload: HandoffPayload): string {
    const jsonStr = JSON.stringify(payload);
    return `${this.SCHEME}?p=${btoa(jsonStr)}`;
  }
}

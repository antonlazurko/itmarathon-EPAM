import type { GetParticipantsResponse } from "@types/api.ts";

export interface ParticipantsListProps {
  participants: GetParticipantsResponse;
  onDeleteUser: (id: number) => Promise<void>;
}

export interface PersonalInformation {
  firstName: string;
  lastName: string;
  phone: string;
  email?: string;
  deliveryInfo: string;
  link?: string;
}

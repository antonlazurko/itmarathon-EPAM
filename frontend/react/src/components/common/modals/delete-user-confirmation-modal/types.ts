import type { Participant } from "@types/api";

export interface DeleteUserConfirmationModalProps {
  isOpen?: boolean;
  onClose: () => void;
  onConfirm: () => void;
  participant: Participant;
}

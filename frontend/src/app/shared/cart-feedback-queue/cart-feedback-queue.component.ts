import { Component, OnDestroy, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

interface CartFeedbackMessage {
  id: number;
  text: string;
}

@Component({
  selector: 'app-cart-feedback-queue',
  imports: [RouterLink],
  templateUrl: './cart-feedback-queue.component.html',
  styleUrl: './cart-feedback-queue.component.css'
})
export class CartFeedbackQueueComponent implements OnDestroy {
  private readonly maxVisibleMessages = 3;
  private readonly messageDurationMs = 5000;
  private readonly pendingMessages: CartFeedbackMessage[] = [];
  private readonly timeoutIds = new Map<number, ReturnType<typeof setTimeout>>();
  private nextMessageId = 1;

  readonly actionLabel = input('View cart');
  readonly actionLink = input<string | unknown[]>('/cart');
  protected readonly visibleMessages = signal<CartFeedbackMessage[]>([]);

  ngOnDestroy(): void {
    for (const timeoutId of this.timeoutIds.values()) {
      clearTimeout(timeoutId);
    }

    this.timeoutIds.clear();
  }

  enqueue(text: string): void {
    const message = {
      id: this.nextMessageId,
      text
    };
    this.nextMessageId += 1;

    if (this.visibleMessages().length < this.maxVisibleMessages) {
      this.showMessage(message);
      return;
    }

    this.pendingMessages.push(message);
  }

  protected dismiss(id: number): void {
    this.removeVisibleMessage(id);
    this.promotePendingMessages();
  }

  private showMessage(message: CartFeedbackMessage): void {
    this.visibleMessages.update((messages) => [...messages, message]);

    const timeoutId = setTimeout(() => {
      this.removeVisibleMessage(message.id);
      this.promotePendingMessages();
    }, this.messageDurationMs);

    this.timeoutIds.set(message.id, timeoutId);
  }

  private removeVisibleMessage(id: number): void {
    const timeoutId = this.timeoutIds.get(id);
    if (timeoutId !== undefined) {
      clearTimeout(timeoutId);
      this.timeoutIds.delete(id);
    }

    this.visibleMessages.update((messages) => messages.filter((message) => message.id !== id));
  }

  private promotePendingMessages(): void {
    while (
      this.visibleMessages().length < this.maxVisibleMessages &&
      this.pendingMessages.length > 0
    ) {
      const nextMessage = this.pendingMessages.shift();
      if (nextMessage !== undefined) {
        this.showMessage(nextMessage);
      }
    }
  }
}
